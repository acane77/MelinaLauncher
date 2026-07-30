using System;
using System.Collections.Generic;

namespace tbm_launcher
{
    public static class AppLanguageCodes
    {
        public const string English = "en";
        public const string SimplifiedChinese = "zh-Hans";
        public const string TraditionalChinese = "zh-Hant";
        public const string Japanese = "ja";
    }

    public static class AppLocalization
    {
        public const string DefaultLanguageCode = AppLanguageCodes.English;

        private static readonly Dictionary<string, string> LanguageAliases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "en", AppLanguageCodes.English },
                { "english", AppLanguageCodes.English },
                { "zh-hans", AppLanguageCodes.SimplifiedChinese },
                { "zh-cn", AppLanguageCodes.SimplifiedChinese },
                { "zh-sg", AppLanguageCodes.SimplifiedChinese },
                { "simplified chinese", AppLanguageCodes.SimplifiedChinese },
                { "simplified-chinese", AppLanguageCodes.SimplifiedChinese },
                { "简体中文", AppLanguageCodes.SimplifiedChinese },
                { "中文简体", AppLanguageCodes.SimplifiedChinese },
                { "zh-hant", AppLanguageCodes.TraditionalChinese },
                { "zh-tw", AppLanguageCodes.TraditionalChinese },
                { "zh-hk", AppLanguageCodes.TraditionalChinese },
                { "traditional chinese", AppLanguageCodes.TraditionalChinese },
                { "traditional-chinese", AppLanguageCodes.TraditionalChinese },
                { "繁體中文", AppLanguageCodes.TraditionalChinese },
                { "中文繁體", AppLanguageCodes.TraditionalChinese },
                { "中文繁体", AppLanguageCodes.TraditionalChinese },
                { "ja", AppLanguageCodes.Japanese },
                { "jp", AppLanguageCodes.Japanese },
                { "japanese", AppLanguageCodes.Japanese },
                { "日本語", AppLanguageCodes.Japanese },
                { "日语", AppLanguageCodes.Japanese }
            };

        private static readonly Dictionary<string, Dictionary<string, string>> Resources =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    AppLanguageCodes.English,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Common.ErrorTitle", "Error" },
                        { "Common.Save", "Save" },
                        { "Common.Cancel", "Cancel" },
                        { "Common.Add", "Add" },
                        { "Common.Delete", "Delete" },
                        { "Common.MoveUp", "Move Up" },
                        { "Common.MoveDown", "Move Down" },
                        { "Common.Copy", "Copy" },
                        { "Common.Refresh", "Refresh" },
                        { "Common.Preview", "Preview" },
                        { "Common.Browse", "Browse" },
                        { "Common.Edit", "Edit" },

                        { "Language.English", "English (Default)" },
                        { "Language.SimplifiedChinese", "Simplified Chinese" },
                        { "Language.TraditionalChinese", "Traditional Chinese" },
                        { "Language.Japanese", "Japanese" },

                        { "Program.HelpText", "Usage: tbm_launcher [-c]\r\n\r\nOptions:\r\n  -c, --config     Open configure window\r\n  -h               Print this help message" },

                        { "Main.BatchStart", "Start All" },
                        { "Main.BatchStop", "Stop All" },
                        { "Main.Refresh", "Refresh" },
                        { "Main.LoadConfigFailedLine", "Failed to load configuration: Line {0}: {1}" },
                        { "Main.LoadConfigFailed", "Failed to load configuration: {0}" },

                        { "Config.SystemEntry", "System" },
                        { "Config.WindowTitle", "Configure {0}" },
                        { "Config.NewItemFormat", "New Item ({0})" },
                        { "Config.NewItemPattern", "^New Item \\((\\d+)\\)$" },
                        { "Config.CopySuffix", "Copy" },
                        { "Config.SystemItemCannotDelete", "The system configuration entry cannot be deleted." },
                        { "Config.SaveFailedLine", "Failed to save configuration: Line {0}: {1}" },
                        { "Config.SaveFailed", "Failed to save configuration: {0}" },
                        { "Config.LoadRawFailedLine", "Failed to load raw configuration: Line {0}: {1}" },
                        { "Config.LoadRawFailed", "Failed to load raw configuration: {0}" },
                        { "Config.PreviewFailed", "Preview failed: {0}" },
                        { "Config.PanelTitle", "Configuration" },

                        { "Config.Field.Title", "Title" },
                        { "Config.Field.CheckInterval", "Refreash Interval (s)" },
                        { "Config.Field.HideOneKey", "Hide Start All and Stop All" },
                        { "Config.Field.Language", "Language" },
                        { "Config.Field.Variables", "Variables" },
                        { "Config.Field.SectionName", "Section Name" },
                        { "Config.Field.Name", "Service Name" },
                        { "Config.Field.Command", "Start Command" },
                        { "Config.Field.StopCommand", "Stop Command" },
                        { "Config.Field.Port", "Port" },
                        { "Config.Field.RequirementCommand", "Dep Check Command" },
                        { "Config.Field.StatusCheckMethod", "Status Check Method" },
                        { "Config.Field.Workdir", "Working Directory" },
                        { "Config.Field.Environs", "Environment Variables" },
                        { "Config.Field.ExecutableName", "Executable Name" },
                        { "Config.Field.RunBackground", "Run in Background" },
                        { "Config.Field.PortClickCommand", "Port Click Command" },
                        { "Config.Field.DisplayBold", "Bold Title" },
                        { "Config.Field.Disabled", "Disable this configure item" },

                        { "Config.Option.RequirementCheckExistence", "Check Whether File Exists" },
                        { "Config.Option.RequirementDoNotCheck", "Do Not Check Dependencies" },
                        { "Config.Option.StatusPortUsage", "Check Port Usage" },
                        { "Config.Option.StatusExecutableExistence", "Check Whether Process Exists" },
                        { "Config.Option.StatusNoCheck", "Do Not Check Running Status" },
                        { "Config.Option.ExecutableNamePrompt", "Enter executable name" },
                        { "Config.Option.ExecutableRegex", "Use regular expression (start and end with /)" },
                        { "Config.Option.PortClickOpenBrowser", "Open In Browser: {0} ({1})" },
                        { "Config.Option.PortClickOpenBrowserDefault", "Open In Browser ({0})" },
                        { "Config.Option.Default", "Default" },

                        { "StringArrayEditor.Title", "Edit String Array" },
                        { "StringMapEditor.Key", "Key" },
                        { "StringMapEditor.Value", "Value" },

                        { "Service.StatusRetrieving", "Retrieving..." },
                        { "Service.StatusRunning", "Running" },
                        { "Service.StatusStopped", "Stopped" },
                        { "Service.StatusMissingDependencies", "Missing Dependencies" },
                        { "Service.StatusProcessing", "Processing" },
                        { "Service.StatusFailed", "Failed" },
                        { "Service.StatusConfigError", "Configuration Error" },
                        { "Service.ButtonStart", "Start" },
                        { "Service.ButtonStop", "Stop" },
                        { "Service.ButtonOpen", "Open" },
                        { "Service.ButtonInstallDependencies", "Install Dependencies" },
                        { "Service.ButtonChangePort", "Change Port" },
                        { "Service.InvalidStatusCheckFormat", "InvalidStatusCheck [{0}]" },
                        { "Service.ErrorCommandFailed", "The configured command for this action failed.\r\n\r\nreturn code: {0}" },
                        { "Service.ErrorPortClickFailed", "The port click action failed.\r\n\r\n{0}" },
                        { "Service.ErrorStopFailed", "The action failed.\r\nPossible reasons:\r\n   (1) The program has already stopped.\r\n   (2) The start command does not contain the actual executable name.\r\n\r\nreturn code: {0}\r\nidentifier: {1}" },

                        { "LaunchInfo.AnalysisError", "An error occurred while parsing the configuration file." },
                        { "LaunchInfo.PortRange", "The valid port range is 0-65535." },
                        { "LaunchInfo.InvalidStatusCheckMethod", "Invalid status check method: {0}. Supported values: {1}, {2}, and {3}." },
                        { "LaunchInfo.NoSuchField", "Unknown configuration field: {0}" },
                        { "LaunchInfo.ErrorValue", "<Error Value>" },
                        { "LaunchInfo.Unnamed", "<Unnamed>" },

                        { "ConfigFile.SystemHideOneKeyMustBeBoolean", "system.hide_one_key must be true/false or 1/0." },
                        { "ConfigFile.SystemChkIntervalMustBeInteger", "system.chk_interval must be an integer." },
                        { "ConfigFile.SystemChkIntervalMustBePositive", "system.chk_interval must be greater than 0." },
                        { "ConfigFile.SystemLanguageInvalid", "system.language must be one of: en, zh-Hans, zh-Hant, ja." },
                        { "ConfigFile.ConfigObjectNull", "The configuration object is null." },
                        { "ConfigFile.SectionIdentifierInvalid", "Invalid section identifier {0}: {1}" },
                        { "ConfigFile.SectionIdentifierDuplicate", "Duplicate section identifier: {0}" },
                        { "ConfigFile.SectionNameEmpty", "It cannot be empty." },
                        { "ConfigFile.SectionNameReserved", "The reserved name system cannot be used." },
                        { "ConfigFile.SectionNamePattern", "Only letters, numbers, underscores, and hyphens are allowed." },
                        { "ConfigFile.DuplicateSection", "Duplicate section: {0}" },
                        { "ConfigFile.InvalidSectionName", "Invalid section name: {0}. {1}" },
                        { "ConfigFile.SectionRequiredBeforeProperties", "Invalid configuration format. Declare a section before properties." },
                        { "ConfigFile.InvalidConfigEntryFormat", "Invalid configuration entry format." },
                        { "ConfigFile.EnvironsMustBeStringArray", "environs must be a string array." },
                        { "ConfigFile.CannotParseValue", "Unable to parse the configuration value." },
                        { "ConfigFile.SystemChkIntervalMustBePositiveInteger", "system.chk_interval must be a positive integer." }
                    }
                },
                {
                    AppLanguageCodes.SimplifiedChinese,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Common.ErrorTitle", "错误" },
                        { "Common.Save", "保存" },
                        { "Common.Cancel", "取消" },
                        { "Common.Add", "添加" },
                        { "Common.Delete", "删除" },
                        { "Common.MoveUp", "上移" },
                        { "Common.MoveDown", "下移" },
                        { "Common.Copy", "复制" },
                        { "Common.Refresh", "刷新" },
                        { "Common.Preview", "预览" },
                        { "Common.Browse", "浏览..." },
                        { "Common.Edit", "编辑..." },

                        { "Language.English", "英文（默认）" },
                        { "Language.SimplifiedChinese", "中文简体" },
                        { "Language.TraditionalChinese", "中文繁体" },
                        { "Language.Japanese", "日语" },

                        { "Program.HelpText", "Usage: tbm_launcher [-c]\r\n\r\nOptions:\r\n  -c, --config     打开配置窗口\r\n  -h               显示此帮助信息" },

                        { "Main.BatchStart", "一键启动" },
                        { "Main.BatchStop", "全部停止" },
                        { "Main.Refresh", "刷新" },
                        { "Main.LoadConfigFailedLine", "加载配置文件失败: Line {0}: {1}" },
                        { "Main.LoadConfigFailed", "加载配置文件失败: {0}" },

                        { "Config.SystemEntry", "系统配置" },
                        { "Config.WindowTitle", "配置 {0}" },
                        { "Config.NewItemFormat", "新建配置项 ({0})" },
                        { "Config.NewItemPattern", "^新建配置项 \\((\\d+)\\)$" },
                        { "Config.CopySuffix", "副本" },
                        { "Config.SystemItemCannotDelete", "系统配置项不可删除。" },
                        { "Config.SaveFailedLine", "保存配置失败: Line {0}: {1}" },
                        { "Config.SaveFailed", "保存配置失败: {0}" },
                        { "Config.LoadRawFailedLine", "加载原始配置失败: Line {0}: {1}" },
                        { "Config.LoadRawFailed", "加载原始配置失败: {0}" },
                        { "Config.PreviewFailed", "预览失败: {0}" },
                        { "Config.PanelTitle", "配置" },

                        { "Config.Field.Title", "标题" },
                        { "Config.Field.CheckInterval", "刷新间隔（秒）" },
                        { "Config.Field.HideOneKey", "隐藏一键启动和一键停止" },
                        { "Config.Field.Language", "语言" },
                        { "Config.Field.Variables", "\u53d8\u91cf" },
                        { "Config.Field.SectionName", "配置标识" },
                        { "Config.Field.Name", "服务名称" },
                        { "Config.Field.Command", "启动命令" },
                        { "Config.Field.StopCommand", "停止命令" },
                        { "Config.Field.Port", "端口" },
                        { "Config.Field.RequirementCommand", "依赖检查命令" },
                        { "Config.Field.StatusCheckMethod", "运行状态检查" },
                        { "Config.Field.Workdir", "工作目录" },
                        { "Config.Field.Environs", "环境变量" },
                        { "Config.Field.ExecutableName", "可执行文件名称" },
                        { "Config.Field.RunBackground", "在后台运行" },
                        { "Config.Field.PortClickCommand", "端口点击命令" },
                        { "Config.Field.DisplayBold", "加粗显示标题" },
                        { "Config.Field.Disabled", "禁用配置项" },

                        { "Config.Option.RequirementCheckExistence", "检查文件是否存在" },
                        { "Config.Option.RequirementDoNotCheck", "不检查依赖" },
                        { "Config.Option.StatusPortUsage", "检查端口占用" },
                        { "Config.Option.StatusExecutableExistence", "检查进程是否存在" },
                        { "Config.Option.StatusNoCheck", "不检查运行状态" },
                        { "Config.Option.ExecutableNamePrompt", "填写可执行文件名称" },
                        { "Config.Option.ExecutableRegex", "使用正则表达式（以/开头和结尾）" },
                        { "Config.Option.PortClickOpenBrowser", "在浏览器打开：{0}（{1}）" },
                        { "Config.Option.PortClickOpenBrowserDefault", "在浏览器打开（{0}）" },
                        { "Config.Option.Default", "默认" },

                        { "StringArrayEditor.Title", "编辑字符串数组" },
                        { "StringMapEditor.Key", "\u952e" },
                        { "StringMapEditor.Value", "\u503c" },

                        { "Service.StatusRetrieving", "获取运行信息..." },
                        { "Service.StatusRunning", "正在运行" },
                        { "Service.StatusStopped", "已停止" },
                        { "Service.StatusMissingDependencies", "缺少依赖" },
                        { "Service.StatusProcessing", "正在处理" },
                        { "Service.StatusFailed", "运行失败" },
                        { "Service.StatusConfigError", "配置文件错误" },
                        { "Service.ButtonStart", "启动" },
                        { "Service.ButtonStop", "停止" },
                        { "Service.ButtonOpen", "打开" },
                        { "Service.ButtonInstallDependencies", "安装依赖" },
                        { "Service.ButtonChangePort", "修改端口" },
                        { "Service.InvalidStatusCheckFormat", "InvalidStatusCheck [{0}]" },
                        { "Service.ErrorCommandFailed", "执行该操作指定的命令时发生了错误。\r\n\r\nreturn code: {0}" },
                        { "Service.ErrorPortClickFailed", "执行端口点击操作时发生了错误。\r\n\r\n{0}" },
                        { "Service.ErrorStopFailed", "执行该操作的时候发生了错误。\r\n可能是因为\r\n   (1)该程序已经停止运行\r\n   (2)该程序的启动命令并不包含实际执行的应用的名称\r\n\r\nreturn code: {0}\r\nidentifier: {1}" },

                        { "LaunchInfo.AnalysisError", "分析配置文件时出现了错误。" },
                        { "LaunchInfo.PortRange", "端口号取值范围是 0-65535。" },
                        { "LaunchInfo.InvalidStatusCheckMethod", "配置项：错误的启动状态检查方式：{0}，可选的值有：{1}、{2} 和 {3}。" },
                        { "LaunchInfo.NoSuchField", "没有此配置项：{0}" },
                        { "LaunchInfo.ErrorValue", "<错误值>" },
                        { "LaunchInfo.Unnamed", "<未命名>" },

                        { "ConfigFile.SystemHideOneKeyMustBeBoolean", "system.hide_one_key 必须是 true/false 或 1/0。" },
                        { "ConfigFile.SystemChkIntervalMustBeInteger", "system.chk_interval 必须是整数。" },
                        { "ConfigFile.SystemChkIntervalMustBePositive", "system.chk_interval 必须大于 0。" },
                        { "ConfigFile.SystemLanguageInvalid", "system.language 必须是以下值之一：en、zh-Hans、zh-Hant、ja。" },
                        { "ConfigFile.ConfigObjectNull", "配置对象为空。" },
                        { "ConfigFile.SectionIdentifierInvalid", "配置标识 {0} 无效: {1}" },
                        { "ConfigFile.SectionIdentifierDuplicate", "配置标识重复: {0}" },
                        { "ConfigFile.SectionNameEmpty", "不能为空。" },
                        { "ConfigFile.SectionNameReserved", "不能使用保留名称 system。" },
                        { "ConfigFile.SectionNamePattern", "只能包含字母、数字、下划线和短横线。" },
                        { "ConfigFile.DuplicateSection", "重复的 section: {0}" },
                        { "ConfigFile.InvalidSectionName", "无效的 section 名称: {0}。{1}" },
                        { "ConfigFile.SectionRequiredBeforeProperties", "配置文件格式错误，需要先声明 section。" },
                        { "ConfigFile.InvalidConfigEntryFormat", "配置项格式错误。" },
                        { "ConfigFile.EnvironsMustBeStringArray", "environs 需要是字符串数组。" },
                        { "ConfigFile.CannotParseValue", "无法解析配置值。" },
                        { "ConfigFile.SystemChkIntervalMustBePositiveInteger", "system.chk_interval 必须是大于 0 的整数。" }
                    }
                },
                {
                    AppLanguageCodes.TraditionalChinese,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Common.ErrorTitle", "錯誤" },
                        { "Common.Save", "儲存" },
                        { "Common.Cancel", "取消" },
                        { "Common.Add", "新增" },
                        { "Common.Delete", "刪除" },
                        { "Common.MoveUp", "上移" },
                        { "Common.MoveDown", "下移" },
                        { "Common.Copy", "複製" },
                        { "Common.Refresh", "重新整理" },
                        { "Common.Preview", "預覽" },
                        { "Common.Browse", "瀏覽..." },
                        { "Common.Edit", "編輯..." },

                        { "Language.English", "英文（預設）" },
                        { "Language.SimplifiedChinese", "中文簡體" },
                        { "Language.TraditionalChinese", "中文繁體" },
                        { "Language.Japanese", "日語" },

                        { "Program.HelpText", "Usage: tbm_launcher [-c]\r\n\r\nOptions:\r\n  -c, --config     開啟設定視窗\r\n  -h               顯示此說明訊息" },

                        { "Main.BatchStart", "一鍵啟動" },
                        { "Main.BatchStop", "全部停止" },
                        { "Main.Refresh", "重新整理" },
                        { "Main.LoadConfigFailedLine", "載入設定檔失敗: Line {0}: {1}" },
                        { "Main.LoadConfigFailed", "載入設定檔失敗: {0}" },

                        { "Config.SystemEntry", "系統設定" },
                        { "Config.WindowTitle", "設定 {0}" },
                        { "Config.NewItemFormat", "新建設定項 ({0})" },
                        { "Config.NewItemPattern", "^新建設定項 \\((\\d+)\\)$" },
                        { "Config.CopySuffix", "副本" },
                        { "Config.SystemItemCannotDelete", "系統設定項不可刪除。" },
                        { "Config.SaveFailedLine", "儲存設定失敗: Line {0}: {1}" },
                        { "Config.SaveFailed", "儲存設定失敗: {0}" },
                        { "Config.LoadRawFailedLine", "載入原始設定失敗: Line {0}: {1}" },
                        { "Config.LoadRawFailed", "載入原始設定失敗: {0}" },
                        { "Config.PreviewFailed", "預覽失敗: {0}" },
                        { "Config.PanelTitle", "設定" },

                        { "Config.Field.Title", "標題" },
                        { "Config.Field.CheckInterval", "重新整理間隔（秒）" },
                        { "Config.Field.HideOneKey", "隱藏一鍵啟動與一鍵停止" },
                        { "Config.Field.Language", "語言" },
                        { "Config.Field.Variables", "\u8b8a\u6578" },
                        { "Config.Field.SectionName", "設定識別" },
                        { "Config.Field.Name", "服務名稱" },
                        { "Config.Field.Command", "啟動命令" },
                        { "Config.Field.StopCommand", "停止命令" },
                        { "Config.Field.Port", "連接埠" },
                        { "Config.Field.RequirementCommand", "相依檢查命令" },
                        { "Config.Field.StatusCheckMethod", "執行狀態檢查" },
                        { "Config.Field.Workdir", "工作目錄" },
                        { "Config.Field.Environs", "環境變數" },
                        { "Config.Field.ExecutableName", "可執行檔名稱" },
                        { "Config.Field.RunBackground", "背景執行" },
                        { "Config.Field.PortClickCommand", "連接埠點擊命令" },
                        { "Config.Field.DisplayBold", "粗體顯示標題" },
                        { "Config.Field.Disabled", "停用設定項" },

                        { "Config.Option.RequirementCheckExistence", "檢查檔案是否存在" },
                        { "Config.Option.RequirementDoNotCheck", "不檢查相依" },
                        { "Config.Option.StatusPortUsage", "檢查連接埠占用" },
                        { "Config.Option.StatusExecutableExistence", "檢查程序是否存在" },
                        { "Config.Option.StatusNoCheck", "不檢查執行狀態" },
                        { "Config.Option.ExecutableNamePrompt", "填寫可執行檔名稱" },
                        { "Config.Option.ExecutableRegex", "使用正則表達式（以/開頭和結尾）" },
                        { "Config.Option.PortClickOpenBrowser", "在瀏覽器開啟：{0}（{1}）" },
                        { "Config.Option.PortClickOpenBrowserDefault", "在瀏覽器開啟（{0}）" },
                        { "Config.Option.Default", "預設" },

                        { "StringArrayEditor.Title", "編輯字串陣列" },
                        { "StringMapEditor.Key", "\u9375" },
                        { "StringMapEditor.Value", "\u503c" },

                        { "Service.StatusRetrieving", "取得執行資訊..." },
                        { "Service.StatusRunning", "正在執行" },
                        { "Service.StatusStopped", "已停止" },
                        { "Service.StatusMissingDependencies", "缺少相依" },
                        { "Service.StatusProcessing", "處理中" },
                        { "Service.StatusFailed", "執行失敗" },
                        { "Service.StatusConfigError", "設定檔錯誤" },
                        { "Service.ButtonStart", "啟動" },
                        { "Service.ButtonStop", "停止" },
                        { "Service.ButtonOpen", "開啟" },
                        { "Service.ButtonInstallDependencies", "安裝相依" },
                        { "Service.ButtonChangePort", "修改連接埠" },
                        { "Service.InvalidStatusCheckFormat", "InvalidStatusCheck [{0}]" },
                        { "Service.ErrorCommandFailed", "執行此操作指定的命令時發生錯誤。\r\n\r\nreturn code: {0}" },
                        { "Service.ErrorPortClickFailed", "執行連接埠點擊操作時發生錯誤。\r\n\r\n{0}" },
                        { "Service.ErrorStopFailed", "執行此操作時發生錯誤。\r\n可能原因：\r\n   (1) 程式已經停止執行\r\n   (2) 啟動命令未包含實際執行檔名稱\r\n\r\nreturn code: {0}\r\nidentifier: {1}" },

                        { "LaunchInfo.AnalysisError", "分析設定檔時發生錯誤。" },
                        { "LaunchInfo.PortRange", "連接埠範圍必須在 0-65535。" },
                        { "LaunchInfo.InvalidStatusCheckMethod", "設定項的執行狀態檢查方式無效：{0}，可用值為：{1}、{2} 與 {3}。" },
                        { "LaunchInfo.NoSuchField", "沒有此設定項：{0}" },
                        { "LaunchInfo.ErrorValue", "<錯誤值>" },
                        { "LaunchInfo.Unnamed", "<未命名>" },

                        { "ConfigFile.SystemHideOneKeyMustBeBoolean", "system.hide_one_key 必須是 true/false 或 1/0。" },
                        { "ConfigFile.SystemChkIntervalMustBeInteger", "system.chk_interval 必須是整數。" },
                        { "ConfigFile.SystemChkIntervalMustBePositive", "system.chk_interval 必須大於 0。" },
                        { "ConfigFile.SystemLanguageInvalid", "system.language 必須是以下值之一：en、zh-Hans、zh-Hant、ja。" },
                        { "ConfigFile.ConfigObjectNull", "設定物件為空。" },
                        { "ConfigFile.SectionIdentifierInvalid", "設定識別 {0} 無效: {1}" },
                        { "ConfigFile.SectionIdentifierDuplicate", "設定識別重複: {0}" },
                        { "ConfigFile.SectionNameEmpty", "不能為空。" },
                        { "ConfigFile.SectionNameReserved", "不能使用保留名稱 system。" },
                        { "ConfigFile.SectionNamePattern", "只能包含字母、數字、底線與連字號。" },
                        { "ConfigFile.DuplicateSection", "重複的 section: {0}" },
                        { "ConfigFile.InvalidSectionName", "無效的 section 名稱: {0}。{1}" },
                        { "ConfigFile.SectionRequiredBeforeProperties", "設定檔格式錯誤，必須先宣告 section。" },
                        { "ConfigFile.InvalidConfigEntryFormat", "設定項格式錯誤。" },
                        { "ConfigFile.EnvironsMustBeStringArray", "environs 必須是字串陣列。" },
                        { "ConfigFile.CannotParseValue", "無法解析設定值。" },
                        { "ConfigFile.SystemChkIntervalMustBePositiveInteger", "system.chk_interval 必須是大於 0 的整數。" }
                    }
                },
                {
                    AppLanguageCodes.Japanese,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Common.ErrorTitle", "エラー" },
                        { "Common.Save", "保存" },
                        { "Common.Cancel", "キャンセル" },
                        { "Common.Add", "追加" },
                        { "Common.Delete", "削除" },
                        { "Common.MoveUp", "上へ" },
                        { "Common.MoveDown", "下へ" },
                        { "Common.Copy", "コピー" },
                        { "Common.Refresh", "更新" },
                        { "Common.Preview", "プレビュー" },
                        { "Common.Browse", "参照..." },
                        { "Common.Edit", "編集..." },

                        { "Language.English", "英語（既定）" },
                        { "Language.SimplifiedChinese", "中国語簡体字" },
                        { "Language.TraditionalChinese", "中国語繁体字" },
                        { "Language.Japanese", "日本語" },

                        { "Program.HelpText", "Usage: tbm_launcher [-c]\r\n\r\nOptions:\r\n  -c, --config     設定ウィンドウを開く\r\n  -h               このヘルプを表示" },

                        { "Main.BatchStart", "一括起動" },
                        { "Main.BatchStop", "一括停止" },
                        { "Main.Refresh", "更新" },
                        { "Main.LoadConfigFailedLine", "設定ファイルの読み込みに失敗しました: Line {0}: {1}" },
                        { "Main.LoadConfigFailed", "設定ファイルの読み込みに失敗しました: {0}" },

                        { "Config.SystemEntry", "システム設定" },
                        { "Config.WindowTitle", "設定 {0}" },
                        { "Config.NewItemFormat", "新しい項目 ({0})" },
                        { "Config.NewItemPattern", "^新しい項目 \\((\\d+)\\)$" },
                        { "Config.CopySuffix", "コピー" },
                        { "Config.SystemItemCannotDelete", "システム設定項目は削除できません。" },
                        { "Config.SaveFailedLine", "設定の保存に失敗しました: Line {0}: {1}" },
                        { "Config.SaveFailed", "設定の保存に失敗しました: {0}" },
                        { "Config.LoadRawFailedLine", "元の設定の読み込みに失敗しました: Line {0}: {1}" },
                        { "Config.LoadRawFailed", "元の設定の読み込みに失敗しました: {0}" },
                        { "Config.PreviewFailed", "プレビューに失敗しました: {0}" },
                        { "Config.PanelTitle", "設定" },

                        { "Config.Field.Title", "タイトル" },
                        { "Config.Field.CheckInterval", "更新間隔（秒）" },
                        { "Config.Field.HideOneKey", "一括起動と一括停止を隠す" },
                        { "Config.Field.Language", "言語" },
                        { "Config.Field.Variables", "\u5909\u6570" },
                        { "Config.Field.SectionName", "設定識別子" },
                        { "Config.Field.Name", "サービス名" },
                        { "Config.Field.Command", "起動コマンド" },
                        { "Config.Field.StopCommand", "停止コマンド" },
                        { "Config.Field.Port", "ポート" },
                        { "Config.Field.RequirementCommand", "依存関係確認コマンド" },
                        { "Config.Field.StatusCheckMethod", "実行状態確認" },
                        { "Config.Field.Workdir", "作業ディレクトリ" },
                        { "Config.Field.Environs", "環境変数" },
                        { "Config.Field.ExecutableName", "実行ファイル名" },
                        { "Config.Field.RunBackground", "バックグラウンド実行" },
                        { "Config.Field.PortClickCommand", "ポートクリックコマンド" },
                        { "Config.Field.DisplayBold", "タイトルを太字で表示" },
                        { "Config.Field.Disabled", "項目を無効化" },

                        { "Config.Option.RequirementCheckExistence", "ファイルの存在を確認" },
                        { "Config.Option.RequirementDoNotCheck", "依存関係を確認しない" },
                        { "Config.Option.StatusPortUsage", "ポート使用状況を確認" },
                        { "Config.Option.StatusExecutableExistence", "プロセスの存在を確認" },
                        { "Config.Option.StatusNoCheck", "実行状態を確認しない" },
                        { "Config.Option.ExecutableNamePrompt", "実行ファイル名を入力" },
                        { "Config.Option.ExecutableRegex", "正規表現を使用（/で開始・終了）" },
                        { "Config.Option.PortClickOpenBrowser", "ブラウザーで開く: {0}（{1}）" },
                        { "Config.Option.PortClickOpenBrowserDefault", "ブラウザーで開く（{0}）" },
                        { "Config.Option.Default", "既定" },

                        { "StringArrayEditor.Title", "文字列配列を編集" },
                        { "StringMapEditor.Key", "\u30ad\u30fc" },
                        { "StringMapEditor.Value", "\u5024" },

                        { "Service.StatusRetrieving", "状態取得中..." },
                        { "Service.StatusRunning", "実行中" },
                        { "Service.StatusStopped", "停止済み" },
                        { "Service.StatusMissingDependencies", "依存関係不足" },
                        { "Service.StatusProcessing", "処理中" },
                        { "Service.StatusFailed", "実行失敗" },
                        { "Service.StatusConfigError", "設定ファイルエラー" },
                        { "Service.ButtonStart", "起動" },
                        { "Service.ButtonStop", "停止" },
                        { "Service.ButtonOpen", "開く" },
                        { "Service.ButtonInstallDependencies", "依存関係をインストール" },
                        { "Service.ButtonChangePort", "ポート変更" },
                        { "Service.InvalidStatusCheckFormat", "InvalidStatusCheck [{0}]" },
                        { "Service.ErrorCommandFailed", "この操作に設定されたコマンドの実行でエラーが発生しました。\r\n\r\nreturn code: {0}" },
                        { "Service.ErrorPortClickFailed", "ポートクリック操作でエラーが発生しました。\r\n\r\n{0}" },
                        { "Service.ErrorStopFailed", "操作の実行中にエラーが発生しました。\r\n原因の可能性:\r\n   (1) プログラムはすでに停止しています\r\n   (2) 起動コマンドに実際の実行ファイル名が含まれていません\r\n\r\nreturn code: {0}\r\nidentifier: {1}" },

                        { "LaunchInfo.AnalysisError", "設定ファイルの解析中にエラーが発生しました。" },
                        { "LaunchInfo.PortRange", "ポート番号は 0-65535 の範囲で指定してください。" },
                        { "LaunchInfo.InvalidStatusCheckMethod", "無効な実行状態確認方法です: {0}。使用可能な値: {1}, {2}, {3}." },
                        { "LaunchInfo.NoSuchField", "不明な設定項目です: {0}" },
                        { "LaunchInfo.ErrorValue", "<エラー値>" },
                        { "LaunchInfo.Unnamed", "<未命名>" },

                        { "ConfigFile.SystemHideOneKeyMustBeBoolean", "system.hide_one_key は true/false または 1/0 である必要があります。" },
                        { "ConfigFile.SystemChkIntervalMustBeInteger", "system.chk_interval は整数である必要があります。" },
                        { "ConfigFile.SystemChkIntervalMustBePositive", "system.chk_interval は 0 より大きい必要があります。" },
                        { "ConfigFile.SystemLanguageInvalid", "system.language は en、zh-Hans、zh-Hant、ja のいずれかである必要があります。" },
                        { "ConfigFile.ConfigObjectNull", "設定オブジェクトが null です。" },
                        { "ConfigFile.SectionIdentifierInvalid", "設定識別子 {0} は無効です: {1}" },
                        { "ConfigFile.SectionIdentifierDuplicate", "設定識別子が重複しています: {0}" },
                        { "ConfigFile.SectionNameEmpty", "空にできません。" },
                        { "ConfigFile.SectionNameReserved", "予約名 system は使用できません。" },
                        { "ConfigFile.SectionNamePattern", "使用できる文字は英字、数字、アンダースコア、ハイフンのみです。" },
                        { "ConfigFile.DuplicateSection", "section が重複しています: {0}" },
                        { "ConfigFile.InvalidSectionName", "無効な section 名です: {0}。{1}" },
                        { "ConfigFile.SectionRequiredBeforeProperties", "設定ファイルの形式が不正です。先に section を宣言してください。" },
                        { "ConfigFile.InvalidConfigEntryFormat", "設定項目の形式が不正です。" },
                        { "ConfigFile.EnvironsMustBeStringArray", "environs は文字列配列である必要があります。" },
                        { "ConfigFile.CannotParseValue", "設定値を解析できません。" },
                        { "ConfigFile.SystemChkIntervalMustBePositiveInteger", "system.chk_interval は 0 より大きい整数である必要があります。" }
                    }
                }
            };

        public static string CurrentLanguageCode { get; private set; } = DefaultLanguageCode;

        public static void SetLanguage(string languageCode)
        {
            string normalized;
            CurrentLanguageCode = TryNormalizeLanguage(languageCode, out normalized)
                ? normalized
                : DefaultLanguageCode;
        }

        public static bool TryNormalizeLanguage(string value, out string normalized)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                normalized = DefaultLanguageCode;
                return true;
            }

            return LanguageAliases.TryGetValue(value.Trim(), out normalized);
        }

        public static string NormalizeLanguage(string value)
        {
            string normalized;
            return TryNormalizeLanguage(value, out normalized)
                ? normalized
                : DefaultLanguageCode;
        }

        public static string Get(string key)
        {
            string value;
            Dictionary<string, string> currentResources;
            if (Resources.TryGetValue(CurrentLanguageCode, out currentResources)
                && currentResources.TryGetValue(key, out value))
            {
                return value;
            }

            Dictionary<string, string> englishResources;
            if (Resources.TryGetValue(DefaultLanguageCode, out englishResources)
                && englishResources.TryGetValue(key, out value))
            {
                return value;
            }

            return key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args ?? new object[0]);
        }
    }
}
