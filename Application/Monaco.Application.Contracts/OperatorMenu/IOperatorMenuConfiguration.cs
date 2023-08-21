using System;

namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    /// <summary>
    /// IOperatorMenuConfiguration
    /// </summary>
    public interface IOperatorMenuConfiguration
    {
        /// <summary>
        /// GetAccessRuleSet
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetAccessRuleSet(IOperatorMenuConfigObject obj);

        /// <summary>
        /// GetAccessRuleSet
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetAccessRuleSet(IOperatorMenuConfigObject obj, string id);

        /// <summary>
        /// GetPrintAccessRuleSet
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetPrintAccessRuleSet(IOperatorMenuConfigObject obj);

        /// <summary>
        /// GetVisible
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool GetVisible(IOperatorMenuConfigObject obj);

        /// <summary>
        /// GetVisible
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool GetVisible(Type type);

        /// <summary>
        /// GetVisible
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        bool GetVisible(string objectName);

        /// <summary>
        /// GetPageName
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetPageName(IOperatorMenuConfigObject obj);

        /// <summary>
        /// GetPrintButtonEnabled
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        bool GetPrintButtonEnabled(IOperatorMenuConfigObject obj, bool defaultValue);

        /// <summary>
        /// GetSetting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="settingName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetSetting<T>(IOperatorMenuPageViewModel page, string settingName, T defaultValue);

        /// <summary>
        /// GetSetting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetSetting<T>(string settingName, T defaultValue);

        /// <summary>
        /// GetSetting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="settingName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetSetting<T>(Type type, string settingName, T defaultValue);

        /// <summary>
        /// GetSetting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeName"></param>
        /// <param name="settingName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetSetting<T>(string typeName, string settingName, T defaultValue);
    }
}
