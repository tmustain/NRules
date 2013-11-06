using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NRules.Fluent.Dsl;
using NRules.Fluent.Expressions;
using NRules.RuleModel;
using NRules.RuleModel.Builders;

namespace NRules.Fluent
{
    internal class ExpressionBuilder : ILeftHandSide, IRightHandSide
    {
        private readonly RuleBuilder _builder;

        public ExpressionBuilder(RuleBuilder builder)
        {
            _builder = builder;
        }

        public ILeftHandSide If<T>(Expression<Func<T>> alias, params Expression<Func<T, bool>>[] conditions)
        {
            var patternSymbol = ExtractSymbol(alias);
            var leftHandSide = _builder.LeftHandSide();

            var patternBuilder = leftHandSide.Pattern(patternSymbol.Type, patternSymbol.Name);
            foreach (var condition in conditions)
            {
                var rewriter = new ConditionRewriter(leftHandSide.Declarations);
                var rewrittenCondition = rewriter.Rewrite(patternBuilder.Declaration, condition);
                patternBuilder.Condition(rewrittenCondition);
            }
            return this;
        }

        public ILeftHandSide Collect<T>(Expression<Func<IEnumerable<T>>> alias, params Expression<Func<T, bool>>[] itemConditions)
        {
            var collectionSymbol = ExtractSymbol(alias);
            var leftHandSide = _builder.LeftHandSide();

            var outerPatternBuilder = leftHandSide.Pattern(collectionSymbol.Type, collectionSymbol.Name);

            var aggregateBuilder = outerPatternBuilder.SourceAggregate();
            aggregateBuilder.CollectionOf(typeof (T));

            var patternBuilder = aggregateBuilder.SourcePattern(typeof (T));
            foreach (var condition in itemConditions)
            {
                var rewriter = new ConditionRewriter(leftHandSide.Declarations);
                var rewrittenCondition = rewriter.Rewrite(patternBuilder.Declaration, condition);
                patternBuilder.Condition(rewrittenCondition);
            }
            return this;
        }

        public ILeftHandSide Exists<T>(params Expression<Func<T, bool>>[] conditions)
        {
            var leftHandSide = _builder.LeftHandSide();

            var existsBuilder = leftHandSide.Group(GroupType.Exists);

            var patternBuilder = existsBuilder.Pattern(typeof (T));
            foreach (var condition in conditions)
            {
                var rewriter = new ConditionRewriter(leftHandSide.Declarations);
                var rewrittenCondition = rewriter.Rewrite(patternBuilder.Declaration, condition);
                patternBuilder.Condition(rewrittenCondition);
            }
            return this;
        }

        public IRightHandSide Do(Expression<Action<IContext>> action)
        {
            var rightHandSide = _builder.RightHandSide();

            var rewriter = new ActionRewriter(rightHandSide.Declarations);
            var rewrittenAction = rewriter.Rewrite(action);
            rightHandSide.Action(rewrittenAction);

            return this;
        }

        private static ParameterExpression ExtractSymbol<T>(Expression<Func<T>> @alias)
        {
            var fieldMember = @alias.Body as MemberExpression;
            if (fieldMember == null)
            {
                throw new InvalidOperationException("Pattern alias must be a variable");
            }
            return Expression.Parameter(fieldMember.Type, fieldMember.Member.Name);
        }
    }
}