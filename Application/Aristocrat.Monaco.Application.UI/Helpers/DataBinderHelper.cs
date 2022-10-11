//------------------------------------------------------------------------------ 
// 
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//----------------------------------------------------------------------------- 

namespace Aristocrat.Monaco.Application.UI.Helpers
{

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    public sealed class DataBinderHelper
    {

        private static readonly char[] expressionPartSeparator = new char[] { '.' };
        private static readonly char[] indexExprStartChars = new char[] { '[', '(' };
        private static readonly char[] indexExprEndChars = new char[] { ']', ')' };


        ///  
        //
        public DataBinderHelper()
        {
        }


        /// 
        ///    Evaluates data binding expressions at runtime. While
        ///       this method is automatically called when you create data bindings in a RAD
        ///       designer, you can also use it declaratively if you want to simplify the casting 
        ///       to a text string to be displayed on a browser. To do so, you must place the
        ///       <%# and %> tags, which are also used in standard ASP.NET data binding, around the data binding expression. 
        ///    This method is particularly useful when data binding against controls that 
        ///       are in a templated list.
        ///     
        ///       Since this method is called at runtime, it can cause performance
        ///       to noticeably slow compared to standard ASP.NET databinding syntax.
        ///       Use this method judiciously.
        ///     
        /// 
        public static object Eval(object container, string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression = expression.Trim();

            if (expression.Length == 0)
            {
                throw new ArgumentNullException("expression");
            }

            if (container == null)
            {
                return null;
            }

            string[] expressionParts = expression.Split(expressionPartSeparator);

            return DataBinderHelper.Eval(container, expressionParts);
        }


        ///  
        /// 
        private static object Eval(object container, string[] expressionParts)
        {

            object prop;
            int i;

            for (prop = container, i = 0; (i < expressionParts.Length) && (prop != null); i++)
            {
                string expr = expressionParts[i];
                bool indexedExpr = expr.IndexOfAny(indexExprStartChars) >= 0;

                if (indexedExpr == false)
                {
                    prop = DataBinderHelper.GetPropertyValue(prop, expr);
                }
                else
                {
                    prop = DataBinderHelper.GetIndexedPropertyValue(prop, expr);
                }
            }

            return prop;
        }


        ///  
        ///     Evaluates data binding expressions at runtime and
        ///       formats the output as text to be displayed in the requesting browser. While this 
        ///       method is automatically called when you create data bindings in a RAD designer,
        ///       you can also use it declaratively if you want to simplify the casting to a text
        ///       string to be displayed on a browser. To do so, you must place the <%# and %> tags, which are also used in standard ASP.NET data binding, around
        ///       the data binding expression. 
        ///    This method is particularly useful when data binding against controls that
        ///       are in a templated list. 
        ///     
        ///       Since this method is called at
        ///       runtime, it can cause performance to noticeably slow compared to standard ASP.NET 
        ///       databinding syntax. Use this method judiciously, particularly when string
        ///       formatting is not required.
        ///    
        ///  
        public static string Eval(object container, string expression, string format)
        {
            object value = DataBinderHelper.Eval(container, expression);

            if ((value == null) || (value == System.DBNull.Value))
            {
                return String.Empty;
            }
            else
            {
                if (String.IsNullOrEmpty(format))
                {
                    return value.ToString();
                }
                else
                {
                    return String.Format(format, value);
                }
            }
        }


        ///  
        /// 
        public static object GetPropertyValue(object container, string propName)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (String.IsNullOrEmpty(propName))
            {
                throw new ArgumentNullException("propName");
            }

            object prop = null;

            // get a PropertyDescriptor using case-insensitive lookup 
            PropertyDescriptor pd = TypeDescriptor.GetProperties(container).Find(propName, true);
            if (pd != null)
            {
                prop = pd.GetValue(container);
            }
            else
            {
                //throw new Exception(SR.GetString(SR.DataBinder_Prop_Not_Found, container.GetType().FullName, propName));
            }

            return prop;
        }


        /// 
        /// 
        public static string GetPropertyValue(object container, string propName, string format)
        {
            object value = DataBinderHelper.GetPropertyValue(container, propName);

            if ((value == null) || (value == System.DBNull.Value))
            {
                return string.Empty;
            }
            else
            {
                if (String.IsNullOrEmpty(format))
                {
                    return value.ToString();
                }
                else
                {
                    return string.Format(format, value);
                }
            }
        }


        /// 
        ///  
        public static object GetIndexedPropertyValue(object container, string expr)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (String.IsNullOrEmpty(expr))
            {
                throw new ArgumentNullException("expr");
            }

            object prop = null;
            bool intIndex = false;

            int indexExprStart = expr.IndexOfAny(indexExprStartChars);
            int indexExprEnd = expr.IndexOfAny(indexExprEndChars, indexExprStart + 1);

            if ((indexExprStart < 0) || (indexExprEnd < 0) ||
                (indexExprEnd == indexExprStart + 1))
            {
               
            }

            string propName = null;
            object indexValue = null;
            string index = expr.Substring(indexExprStart + 1, indexExprEnd - indexExprStart - 1).Trim();

            if (indexExprStart != 0)
                propName = expr.Substring(0, indexExprStart);

            if (index.Length != 0)
            {
                if (((index[0] == '"') && (index[index.Length - 1] == '"')) ||
                    ((index[0] == '\'') && (index[index.Length - 1] == '\'')))
                {
                    indexValue = index.Substring(1, index.Length - 2);
                }
                else
                {
                    if (Char.IsDigit(index[0]))
                    {
                        // treat it as a number
                        int parsedIndex;
                        intIndex = Int32.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedIndex);
                        if (intIndex)
                        {
                            indexValue = parsedIndex;
                        }
                        else
                        {
                            indexValue = index;
                        }
                    }
                    else
                    {
                        // treat as a string 
                        indexValue = index;
                    }
                }
            }

            if (indexValue == null)
            {
                //throw new ArgumentException(SR.GetString(SR.DataBinder_Invalid_Indexed_Expr, expr));
            }

            object collectionProp = null;
            if ((propName != null) && (propName.Length != 0))
            {
                collectionProp = DataBinderHelper.GetPropertyValue(container, propName);
            }
            else
            {
                collectionProp = container;
            }

            if (collectionProp != null)
            {
                Array arrayProp = collectionProp as Array;
                if (arrayProp != null && intIndex)
                {
                    prop = arrayProp.GetValue((int)indexValue);
                }
                else if ((collectionProp is IList) && intIndex)
                {
                    prop = ((IList)collectionProp)[(int)indexValue];
                }
                else
                {
                    PropertyInfo propInfo =
                        collectionProp.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, new Type[] { indexValue.GetType() }, null);
                    if (propInfo != null)
                    {
                        prop = propInfo.GetValue(collectionProp, new object[] { indexValue });
                    }
                    else
                    {
                        //throw new ArgumentException(SR.GetString(SR.DataBinder_No_Indexed_Accessor, collectionProp.GetType().FullName));
                    }
                }
            }

            return prop;
        }


        /// 
        /// 
        public static string GetIndexedPropertyValue(object container, string propName, string format)
        {
            object value = DataBinderHelper.GetIndexedPropertyValue(container, propName);

            if ((value == null) || (value == System.DBNull.Value))
            {
                return String.Empty;
            }
            else
            {
                if (String.IsNullOrEmpty(format))
                {
                    return value.ToString();
                }
                else
                {
                    return string.Format(format, value);
                }
            }
        }


        /// 
        /// 
        //public static object GetDataItem(object container)
        //{
        //    bool foundDataItem;
        //    return DataBinderHelper.GetDataItem(container, out foundDataItem);
        //}


        /// 
        /// 
        //public static object GetDataItem(object container, out bool foundDataItem)
        //{
        //    // 
        //    if (container == null)
        //    {
        //        foundDataItem = false;
        //        return null;
        //    }

        //    // If object implements IDataItemContainer, get value directly from interface
        //    IDataItemContainer dataItemContainer = container as IDataItemContainer;
        //    if (dataItemContainer != null)
        //    {
        //        foundDataItem = true;
        //        return dataItemContainer.DataItem;
        //    }

        //    // Otherwise, look for a property named "DataItem"
        //    string dataItemPropertyName = "DataItem";

        //    // Check whether the property exists
        //    PropertyInfo propInfo = container.GetType().GetProperty(dataItemPropertyName,
        //        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        //    // If not, return null 
        //    // 
        //    if (propInfo == null)
        //    {
        //        foundDataItem = false;
        //        return null;
        //    }

        //    // If so, get the value 
        //    foundDataItem = true;
        //    return propInfo.GetValue(container, null);
        //}

        ///  
        /// Returns true if the value is null, DBNull, or INullableValue.HasValue is false.
        /// 
        internal static bool IsNull(object value)
        {
            if (value == null || Convert.IsDBNull(value))
            {
                return true;
            }
            return false;
        }
    }
}