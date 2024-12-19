using Microsoft.AspNetCore.SignalR;

namespace ss_blog_be.Common.SQLBuilder.Enums
{
    public enum SQLBuilderOperatorsEnum
    {
        EQUAL,
        NOT_EQUAL,
        LESS_THAN,
        GREATER_THAN,
        LESS_THAN_EQUAL,
        GREATER_THAN_EQUAL,
        IN,
        AND,
        OR,
        NOT,
        LIKE
    }

    public enum SQLBuilderJoinTypeEnum
    {
        INNER,
        LEFT,
        CROSS
    }


    public static class SQLBuilderEnumsTo
    {
        public static string GetOperatorAsStr(SQLBuilderOperatorsEnum op)
        {
            return op switch
            {
                SQLBuilderOperatorsEnum.EQUAL => " = ",
                SQLBuilderOperatorsEnum.NOT_EQUAL => " != ",
                SQLBuilderOperatorsEnum.LESS_THAN => " < ",
                SQLBuilderOperatorsEnum.GREATER_THAN => " > ",
                SQLBuilderOperatorsEnum.LESS_THAN_EQUAL => " <= ",
                SQLBuilderOperatorsEnum.GREATER_THAN_EQUAL => " >= ",
                SQLBuilderOperatorsEnum.IN => " IN ",
                SQLBuilderOperatorsEnum.AND => " AND ",
                SQLBuilderOperatorsEnum.OR => " OR ",
                SQLBuilderOperatorsEnum.NOT => " NOT ",
                SQLBuilderOperatorsEnum.LIKE => " LIKE ",
                _ => throw new NotImplementedException()
            };

        }

        public static string GetFunctionsAsStr(SQLBuilderFunctions op)
        {
            return op switch
            {
                SQLBuilderFunctions.COUNT => " COUNT",
                SQLBuilderFunctions.MAX => " MAX",
                SQLBuilderFunctions.MIN => " MIN",
                SQLBuilderFunctions.AVG => " AVG",
                SQLBuilderFunctions.SUM => " SUM",
                SQLBuilderFunctions.RANDOM => " RANDOM" , 
                SQLBuilderFunctions.ABS => " ABS",
                SQLBuilderFunctions.UPPER => " UPPER",
                SQLBuilderFunctions.LOWER => " LOWER",
                SQLBuilderFunctions.LENGTH => " LENGTH",
                _ => throw new NotImplementedException()
            };
        }

        public static string GetJoinTypeAsStr(SQLBuilderJoinTypeEnum op)
        {
            return op switch
            {
                SQLBuilderJoinTypeEnum.INNER => " INNER JOIN ",
                SQLBuilderJoinTypeEnum.LEFT => " LEFT JOIN ",
                SQLBuilderJoinTypeEnum.CROSS => " CROSS JOIN ",
                _ => throw new NotImplementedException()
            };
        }
    }
}
