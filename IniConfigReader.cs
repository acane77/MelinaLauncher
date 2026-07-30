using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace tbm_launcher
{
    public class LauncherConfigException : Exception
    {
        public int LineNumber { get; private set; }

        public LauncherConfigException(string message, int lineNumber = -1)
            : base(message)
        {
            LineNumber = lineNumber;
        }
    }

    public class LauncherAppConfig
    {
        public string Title = ProgramGlobalConfig.DefaultApplicationTitle;
        public bool HideOneKey = false;
        public string RawHideOneKeyValue;
        public int CheckIntervalSeconds = 10;
        public string RawCheckIntervalValue;
        public string Language = AppLocalization.DefaultLanguageCode;
        public string RawLanguageValue;
        public Dictionary<string, string> Variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<LaunchInfoData> Items = new List<LaunchInfoData>();

        public void SetSystemFieldFromString(string settingName, string value)
        {
            switch (settingName)
            {
                case "title":
                    Title = value ?? ProgramGlobalConfig.DefaultApplicationTitle;
                    break;
                case "hide_one_key":
                    RawHideOneKeyValue = value;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        HideOneKey = false;
                        break;
                    }

                    if (string.Equals(value, "1", StringComparison.Ordinal)
                        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        HideOneKey = true;
                        break;
                    }

                    if (string.Equals(value, "0", StringComparison.Ordinal)
                        || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        HideOneKey = false;
                        break;
                    }

                    throw new LauncherConfigException(AppLocalization.Get("ConfigFile.SystemHideOneKeyMustBeBoolean"));
                case "chk_interval":
                    RawCheckIntervalValue = value;
                    int parsedSeconds;
                    if (!Int32.TryParse(value, out parsedSeconds))
                    {
                        throw new LauncherConfigException(AppLocalization.Get("ConfigFile.SystemChkIntervalMustBeInteger"));
                    }

                    if (parsedSeconds <= 0)
                    {
                        throw new LauncherConfigException(AppLocalization.Get("ConfigFile.SystemChkIntervalMustBePositive"));
                    }

                    CheckIntervalSeconds = parsedSeconds;
                    break;
                case "language":
                    RawLanguageValue = value;
                    string normalizedLanguage;
                    if (!AppLocalization.TryNormalizeLanguage(value, out normalizedLanguage))
                    {
                        throw new LauncherConfigException(AppLocalization.Get("ConfigFile.SystemLanguageInvalid"));
                    }

                    Language = normalizedLanguage;
                    break;
                case "var":
                    break;
            }
        }

        public void SetSystemFieldFromRawString(string settingName, string value)
        {
            switch (settingName)
            {
                case "title":
                    Title = value ?? ProgramGlobalConfig.DefaultApplicationTitle;
                    break;
                case "hide_one_key":
                    RawHideOneKeyValue = value;
                    HideOneKey = string.Equals(value, "1", StringComparison.Ordinal)
                        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    break;
                case "chk_interval":
                    RawCheckIntervalValue = value;
                    int parsedSeconds;
                    if (Int32.TryParse(value, out parsedSeconds) && parsedSeconds > 0)
                    {
                        CheckIntervalSeconds = parsedSeconds;
                    }
                    else
                    {
                        CheckIntervalSeconds = 10;
                    }
                    break;
                case "language":
                    RawLanguageValue = value;
                    Language = AppLocalization.NormalizeLanguage(value);
                    break;
                case "var":
                    break;
            }
        }

        public string GetSystemFieldValueString(string settingName)
        {
            switch (settingName)
            {
                case "title":
                    return Title;
                case "hide_one_key":
                    return RawHideOneKeyValue ?? (HideOneKey ? "1" : "0");
                case "chk_interval":
                    return RawCheckIntervalValue ?? CheckIntervalSeconds.ToString();
                case "language":
                    return RawLanguageValue ?? Language;
                case "var":
                    return LauncherConfigFile.FormatStringMap(Variables);
            }

            return string.Empty;
        }
    }


    static class LauncherConfigFile
    {
        private const int MaxExpansionRounds = 50;
        private static readonly Regex PlaceholderRegex = new Regex(@"\{([A-Za-z0-9_\-.,]+)\}", RegexOptions.Compiled);
        private static readonly Regex SectionNameRegex = new Regex(@"^[A-Za-z0-9_-]+$", RegexOptions.Compiled);

        public static LauncherAppConfig CreateDefaultConfig()
        {
            var config = new LauncherAppConfig();
            config.Items.Add(CreateDefaultItem(
                "config_title",
                "Melina Launcher",
                true,
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                null,
                StatusCheckMethodEnum.NO_CHECK,
                string.Empty,
                string.Empty,
                false));
            config.Items.Add(CreateDefaultItem(
                "config_about",
                "View Source Code and Manual on GitHub",
                false,
                false,
                "explorer.exe https://github.com/acane77/MelinaLauncher",
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                null,
                StatusCheckMethodEnum.NO_CHECK,
                string.Empty,
                string.Empty,
                true));
            config.Items.Add(CreateDefaultItem(
                "config_license",
                "About {config_title.name}",
                true,
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                new[] { "sdsdsds" },
                StatusCheckMethodEnum.NO_CHECK,
                string.Empty,
                string.Empty,
                false));
            config.Items.Add(CreateDefaultItem(
                "config_mit_license",
                "License: MIT License",
                false,
                false,
                "{config_about.command}/blob/master/LICENSE",
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                null,
                StatusCheckMethodEnum.NO_CHECK,
                string.Empty,
                string.Empty,
                true));
            return config;
        }

        public static LauncherAppConfig Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return CreateDefaultConfig();
            }

            string rawText = File.ReadAllText(filePath, Encoding.UTF8);
            string expandedText = Expand(rawText ?? string.Empty);
            return Parse(expandedText, false);
        }

        public static LauncherAppConfig LoadRaw(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return CreateDefaultConfig();
            }

            string rawText = File.ReadAllText(filePath, Encoding.UTF8);
            return Parse(rawText ?? string.Empty, true);
        }

        public static void Save(string filePath, LauncherAppConfig config)
        {
            ValidateForSave(config);
            File.WriteAllText(filePath, Serialize(config), Encoding.UTF8);
        }

        public static string Serialize(LauncherAppConfig config)
        {
            ValidateForSave(config);

            var builder = new StringBuilder();
            builder.AppendLine("[system]");
            builder.Append("title = ").AppendLine(ToTomlString(config.Title ?? string.Empty));
            builder.Append("hide_one_key = ").AppendLine(SerializeBooleanValue(config.GetSystemFieldValueString("hide_one_key")));
            builder.Append("chk_interval = ").AppendLine(SerializePositiveIntegerValue(config.GetSystemFieldValueString("chk_interval"), 10));
            builder.Append("language = ").AppendLine(ToTomlString(AppLocalization.NormalizeLanguage(config.GetSystemFieldValueString("language"))));
            if (config.Variables != null && config.Variables.Count > 0)
            {
                builder.Append("var = ").AppendLine(SerializeStringMap(config.Variables));
            }

            foreach (LaunchInfoData item in config.Items)
            {
                builder.AppendLine();
                builder.Append('[').Append(item.SectionName).AppendLine("]");
                builder.Append("name = ").AppendLine(ToTomlString(item.Name ?? string.Empty));
                builder.Append("display_bold = ").AppendLine(SerializeBooleanValue(item.GetFieldValueString("display_bold")));
                builder.Append("disabled = ").AppendLine(SerializeBooleanValue(item.GetFieldValueString("disabled")));
                builder.Append("command = ").AppendLine(ToTomlString(item.Command ?? string.Empty));
                builder.Append("stop_command = ").AppendLine(ToTomlString(item.StopCommand ?? string.Empty));
                builder.Append("port_click_command = ").AppendLine(ToTomlString(item.PortClickCommand ?? string.Empty));
                builder.Append("port = ").AppendLine(SerializePortValue(item));
                builder.Append("requirement_command = ").AppendLine(ToTomlString(item.RequirementCommand ?? string.Empty));
                if (item.Environs != null && item.Environs.Count > 0)
                {
                    builder.Append("environs = ").AppendLine(ToTomlStringArray(item.Environs));
                }
                builder.Append("status_check_method = ").AppendLine(ToTomlString(item.GetFieldValueString("status_check_method") ?? string.Empty));
                builder.Append("workdir = ").AppendLine(ToTomlString(item.WorkingDirectory ?? string.Empty));
                builder.Append("executable_name = ").AppendLine(ToTomlString(item.ExecutableName ?? string.Empty));
                builder.Append("run_background = ").AppendLine(SerializeRunBackgroundValue(item));
            }

            return builder.ToString();
        }

        public static string Expand(string text)
        {
            string current = text ?? string.Empty;

            for (int round = 0; round < MaxExpansionRounds; round++)
            {
                Dictionary<string, string> values = BuildValueMap(current);
                string next = ExpandOneRound(current, values);
                if (string.Equals(current, next, StringComparison.Ordinal))
                {
                    return current;
                }
                current = next;
            }

            return current;
        }

        public static void ValidateForSave(LauncherAppConfig config)
        {
            if (config == null)
            {
                throw new LauncherConfigException(AppLocalization.Get("ConfigFile.ConfigObjectNull"));
            }

            ValidateSystemSettingsForSave(config);

            var usedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (LaunchInfoData item in config.Items)
            {
                string validationMessage;
                if (!TryValidateSectionName(item.SectionName, out validationMessage))
                {
                    throw new LauncherConfigException(AppLocalization.Format("ConfigFile.SectionIdentifierInvalid", item.SectionName ?? "<empty>", validationMessage));
                }

                if (!usedSections.Add(item.SectionName))
                {
                    throw new LauncherConfigException(AppLocalization.Format("ConfigFile.SectionIdentifierDuplicate", item.SectionName));
                }
            }
        }

        public static bool TryValidateSectionName(string sectionName, out string message)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                message = AppLocalization.Get("ConfigFile.SectionNameEmpty");
                return false;
            }

            if (string.Equals(sectionName, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase))
            {
                message = AppLocalization.Get("ConfigFile.SectionNameReserved");
                return false;
            }

            if (!SectionNameRegex.IsMatch(sectionName))
            {
                message = AppLocalization.Get("ConfigFile.SectionNamePattern");
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static string CreateUniqueSectionName(IEnumerable<string> existingNames, string candidate)
        {
            string seed = SanitizeSectionNameCandidate(candidate);
            var usedNames = new HashSet<string>((existingNames ?? Enumerable.Empty<string>()).Where(n => !string.IsNullOrWhiteSpace(n)), StringComparer.OrdinalIgnoreCase);

            if (!usedNames.Contains(seed) && !string.Equals(seed, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase))
            {
                return seed;
            }

            for (int index = 1; index < 10000; index++)
            {
                string generated = seed + "_" + index;
                if (!usedNames.Contains(generated) && !string.Equals(generated, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase))
                {
                    return generated;
                }
            }

            return seed + "_copy";
        }

        public static string CreateRandomSectionName(IEnumerable<string> existingNames)
        {
            var usedNames = new HashSet<string>((existingNames ?? Enumerable.Empty<string>()).Where(n => !string.IsNullOrWhiteSpace(n)), StringComparer.OrdinalIgnoreCase);

            while (true)
            {
                string candidate = "config_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                if (!usedNames.Contains(candidate))
                {
                    return candidate;
                }
            }
        }

        public static string SanitizeSectionNameCandidate(string candidate)
        {
            string source = string.IsNullOrWhiteSpace(candidate) ? "config_item" : candidate.Trim();
            var builder = new StringBuilder(source.Length);

            foreach (char ch in source)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')
                {
                    builder.Append(ch);
                }
                else
                {
                    builder.Append('_');
                }
            }

            string sanitized = builder.ToString().Trim('_');
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "config_item";
            }
            if (string.Equals(sanitized, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase))
            {
                sanitized = "config_item";
            }

            return sanitized;
        }

        private static LaunchInfoData CreateDefaultItem(
            string sectionName,
            string name,
            bool displayBold,
            bool disabled,
            string command,
            string stopCommand,
            string portClickCommand,
            int port,
            string requirementCommand,
            IEnumerable<string> environs,
            string statusCheckMethod,
            string workingDirectory,
            string executableName,
            bool runBackground)
        {
            var item = new LaunchInfoData
            {
                SectionName = sectionName ?? string.Empty,
                Name = name ?? string.Empty,
                DisplayBold = displayBold,
                Disabled = disabled,
                Command = command ?? string.Empty,
                StopCommand = stopCommand ?? string.Empty,
                PortClickCommand = portClickCommand ?? string.Empty,
                PortNumber = port,
                RequirementCommand = requirementCommand ?? string.Empty,
                StatusCheckMethod = statusCheckMethod ?? StatusCheckMethodEnum.NO_CHECK,
                WorkingDirectory = workingDirectory ?? string.Empty,
                ExecutableName = executableName ?? string.Empty,
                RunBackground = runBackground
            };
            item.SetEnvirons(environs);
            return item;
        }

        private static LauncherAppConfig Parse(string text, bool preserveRawValues)
        {
            var config = new LauncherAppConfig();
            LaunchInfoData currentItem = null;
            string currentSection = null;
            var knownSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] lines = SplitLines(text);

            for (int index = 0; index < lines.Length; index++)
            {
                int lineNumber = index + 1;
                string line = lines[index];
                string trimmed = line.Trim();

                if (trimmed.Length == 0 || trimmed.StartsWith("#"))
                {
                    continue;
                }

                string sectionName;
                if (TryParseSectionHeader(trimmed, out sectionName))
                {
                    if (!knownSections.Add(sectionName))
                    {
                        throw new LauncherConfigException(AppLocalization.Format("ConfigFile.DuplicateSection", sectionName), lineNumber);
                    }

                    currentSection = sectionName;
                    if (string.Equals(sectionName, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase))
                    {
                        currentItem = null;
                    }
                    else
                    {
                        string validationMessage;
                        if (!TryValidateSectionName(sectionName, out validationMessage))
                        {
                            throw new LauncherConfigException(AppLocalization.Format("ConfigFile.InvalidSectionName", sectionName, validationMessage), lineNumber);
                        }

                        currentItem = new LaunchInfoData();
                        currentItem.SectionName = sectionName;
                        currentItem.Name = sectionName;
                        config.Items.Add(currentItem);
                    }
                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentSection))
                {
                    throw new LauncherConfigException(AppLocalization.Get("ConfigFile.SectionRequiredBeforeProperties"), lineNumber);
                }

                string key;
                string rawValue;
                if (!TrySplitKeyValueLine(line, out key, out rawValue))
                {
                    throw new LauncherConfigException(AppLocalization.Get("ConfigFile.InvalidConfigEntryFormat"), lineNumber);
                }

                string loweredKey = key.Trim().ToLowerInvariant();

                if (string.Equals(currentSection, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase))
                {
                    if (loweredKey == "var")
                    {
                        Dictionary<string, string> variables;
                        if (!TryParseStringMap(rawValue, out variables))
                        {
                            throw new LauncherConfigException(AppLocalization.Get("ConfigFile.CannotParseValue"), lineNumber);
                        }
                        config.Variables = variables;
                        continue;
                    }

                    string value = ParseScalarValue(rawValue, lineNumber);
                    if (preserveRawValues)
                    {
                        config.SetSystemFieldFromRawString(loweredKey, value);
                    }
                    else
                    {
                        config.SetSystemFieldFromString(loweredKey, value);
                    }
                    continue;
                }

                try
                {
                    if (loweredKey == "environs")
                    {
                        List<string> environs;
                        if (!TryParseStringArray(rawValue, out environs))
                        {
                            throw new LauncherConfigException(AppLocalization.Get("ConfigFile.EnvironsMustBeStringArray"), lineNumber);
                        }
                        currentItem.SetEnvirons(environs);
                    }
                    else
                    {
                        string value = ParseScalarValue(rawValue, lineNumber);
                        if (preserveRawValues)
                        {
                            currentItem.SetFieldsFromRawString(loweredKey, value);
                        }
                        else
                        {
                            currentItem.SetFieldsFromString(loweredKey, value);
                        }
                    }
                }
                catch (InvalidConfigureValueError ex)
                {
                    throw new LauncherConfigException(ex.Message, lineNumber);
                }
            }

            return config;
        }

        private static Dictionary<string, string> BuildValueMap(string text)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] lines = SplitLines(text);
            string currentSection = null;

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#"))
                {
                    continue;
                }

                string sectionName;
                if (TryParseSectionHeader(trimmed, out sectionName))
                {
                    currentSection = sectionName;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentSection))
                {
                    continue;
                }

                string key;
                string rawValue;
                if (!TrySplitKeyValueLine(line, out key, out rawValue))
                {
                    continue;
                }

                List<string> parsedArray;
                if (TryParseStringArray(rawValue, out parsedArray))
                {
                    string joinedValue = string.Join(";", parsedArray);
                    string currentKey = currentSection + "." + key.Trim();
                    values[currentKey] = joinedValue;
                    for (int arrayIndex = 0; arrayIndex < parsedArray.Count; arrayIndex++)
                    {
                        values[currentKey + "," + arrayIndex] = parsedArray[arrayIndex];
                    }
                    continue;
                }

                Dictionary<string, string> parsedMap;
                if (TryParseStringMap(rawValue, out parsedMap))
                {
                    string currentKey = currentSection + "." + key.Trim();
                    string formattedMap = FormatStringMap(parsedMap);
                    values[currentKey] = formattedMap;
                    if (string.Equals(currentSection, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(key.Trim(), "var", StringComparison.OrdinalIgnoreCase))
                    {
                        values["system.vars"] = formattedMap;
                        foreach (KeyValuePair<string, string> item in parsedMap)
                        {
                            values["system.vars," + item.Key] = item.Value;
                            values["system.var," + item.Key] = item.Value;
                        }
                    }
                    continue;
                }

                string parsedValue;
                if (!TryParseScalarValue(rawValue, out parsedValue))
                {
                    continue;
                }

                values[currentSection + "." + key.Trim()] = parsedValue;
            }

            return values;
        }

        private static string ExpandOneRound(string text, Dictionary<string, string> values)
        {
            string[] lines = SplitLines(text);
            string currentSection = null;
            var output = new List<string>(lines.Length);

            for (int index = 0; index < lines.Length; index++)
            {
                string originalLine = lines[index];
                string key;
                string rawValue;
                if (string.Equals(currentSection, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase)
                    && TrySplitKeyValueLine(originalLine, out key, out rawValue)
                    && string.Equals(key, "var", StringComparison.OrdinalIgnoreCase))
                {
                    Dictionary<string, string> map;
                    if (TryParseStringMap(rawValue, out map))
                    {
                        foreach (string mapKey in map.Keys.ToList())
                        {
                            string mapValue = map[mapKey] ?? string.Empty;
                            map[mapKey] = PlaceholderRegex.Replace(mapValue, match =>
                            {
                                string resolved = ResolveToken(match.Groups[1].Value, currentSection, values);
                                return resolved ?? match.Value;
                            });
                        }
                        output.Add(key + " = " + SerializeStringMap(map));
                        continue;
                    }
                }

                string expandedLine = PlaceholderRegex.Replace(originalLine, match =>
                {
                    string token = match.Groups[1].Value;
                    string resolved = ResolveToken(token, currentSection, values);
                    return resolved ?? match.Value;
                });

                string expandedTrimmed = expandedLine.Trim();
                string sectionName;
                if (TryParseSectionHeader(expandedTrimmed, out sectionName))
                {
                    currentSection = sectionName;
                }

                output.Add(expandedLine);
            }

            return string.Join("\r\n", output);
        }

        private static string ResolveToken(string token, string currentSection, Dictionary<string, string> values)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            if (token.StartsWith("this.", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(currentSection))
                {
                    return null;
                }

                string propertyName = token.Substring(5);
                string localKey = currentSection + "." + propertyName;
                string localValue;
                if (values.TryGetValue(localKey, out localValue))
                {
                    return localValue;
                }
                return null;
            }

            string directValue;
            if (values.TryGetValue(token, out directValue))
            {
                return directValue;
            }

            return null;
        }

        private static bool TrySplitKeyValueLine(string line, out string key, out string value)
        {
            key = null;
            value = null;

            bool inDoubleQuote = false;
            bool inSingleQuote = false;
            bool escaping = false;

            for (int index = 0; index < line.Length; index++)
            {
                char ch = line[index];
                if (escaping)
                {
                    escaping = false;
                    continue;
                }

                if (ch == '\\' && inDoubleQuote)
                {
                    escaping = true;
                    continue;
                }

                if (ch == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (ch == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    continue;
                }

                if (ch == '=' && !inDoubleQuote && !inSingleQuote)
                {
                    key = line.Substring(0, index).Trim();
                    value = line.Substring(index + 1);
                    return key.Length > 0;
                }
            }

            return false;
        }

        private static bool TryParseSectionHeader(string trimmedLine, out string sectionName)
        {
            sectionName = null;
            if (!trimmedLine.StartsWith("["))
            {
                return false;
            }

            int endIndex = trimmedLine.IndexOf(']');
            if (endIndex <= 1)
            {
                return false;
            }

            string remainder = trimmedLine.Substring(endIndex + 1).Trim();
            if (remainder.Length > 0 && !remainder.StartsWith("#"))
            {
                return false;
            }

            sectionName = trimmedLine.Substring(1, endIndex - 1).Trim();
            return sectionName.Length > 0;
        }

        private static string ParseScalarValue(string rawValue, int lineNumber)
        {
            string parsed;
            if (!TryParseScalarValue(rawValue, out parsed))
            {
                throw new LauncherConfigException(AppLocalization.Get("ConfigFile.CannotParseValue"), lineNumber);
            }
            return parsed;
        }

        private static bool TryParseScalarValue(string rawValue, out string parsedValue)
        {
            parsedValue = string.Empty;
            string trimmed = StripInlineComment(rawValue).Trim();

            if (trimmed.Length == 0)
            {
                return true;
            }

            if (trimmed.StartsWith("\"", StringComparison.Ordinal))
            {
                return TryParseDoubleQuotedString(trimmed, out parsedValue);
            }

            if (trimmed.StartsWith("'", StringComparison.Ordinal))
            {
                if (trimmed.Length >= 2 && trimmed.EndsWith("'", StringComparison.Ordinal))
                {
                    parsedValue = trimmed.Substring(1, trimmed.Length - 2);
                    return true;
                }
                return false;
            }

            parsedValue = trimmed;
            return true;
        }

        private static bool TryParseStringArray(string rawValue, out List<string> parsedValues)
        {
            parsedValues = null;
            string trimmed = StripInlineComment(rawValue).Trim();
            if (!trimmed.StartsWith("[", StringComparison.Ordinal) || !trimmed.EndsWith("]", StringComparison.Ordinal))
            {
                return false;
            }

            string content = trimmed.Substring(1, trimmed.Length - 2).Trim();
            parsedValues = new List<string>();
            if (content.Length == 0)
            {
                return true;
            }

            var currentItem = new StringBuilder();
            bool inDoubleQuote = false;
            bool inSingleQuote = false;
            bool escaping = false;
            var rawItems = new List<string>();

            for (int index = 0; index < content.Length; index++)
            {
                char ch = content[index];
                currentItem.Append(ch);

                if (escaping)
                {
                    escaping = false;
                    continue;
                }

                if (ch == '\\' && inDoubleQuote)
                {
                    escaping = true;
                    continue;
                }

                if (ch == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (ch == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    continue;
                }

                if (ch == ',' && !inDoubleQuote && !inSingleQuote)
                {
                    rawItems.Add(currentItem.ToString(0, currentItem.Length - 1));
                    currentItem.Clear();
                }
            }

            if (inDoubleQuote || inSingleQuote || escaping)
            {
                parsedValues = null;
                return false;
            }

            if (currentItem.Length > 0)
            {
                rawItems.Add(currentItem.ToString());
            }

            foreach (string rawItem in rawItems)
            {
                string parsedItem;
                if (!TryParseScalarValue(rawItem, out parsedItem))
                {
                    parsedValues = null;
                    return false;
                }
                parsedValues.Add(parsedItem);
            }

            return true;
        }

        private static bool TryParseStringMap(string rawValue, out Dictionary<string, string> parsedValues)
        {
            parsedValues = null;
            string trimmed = StripInlineComment(rawValue).Trim();
            if (!trimmed.StartsWith("{", StringComparison.Ordinal) || !trimmed.EndsWith("}", StringComparison.Ordinal))
            {
                return false;
            }

            string content = trimmed.Substring(1, trimmed.Length - 2).Trim();
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (content.Length == 0)
            {
                parsedValues = result;
                return true;
            }

            var rawItems = new List<string>();
            var currentItem = new StringBuilder();
            bool inDoubleQuote = false;
            bool inSingleQuote = false;
            bool escaping = false;
            for (int index = 0; index < content.Length; index++)
            {
                char ch = content[index];
                currentItem.Append(ch);
                if (escaping)
                {
                    escaping = false;
                    continue;
                }
                if (ch == '\\' && inDoubleQuote)
                {
                    escaping = true;
                    continue;
                }
                if (ch == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }
                if (ch == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    continue;
                }
                if (ch == ',' && !inDoubleQuote && !inSingleQuote)
                {
                    rawItems.Add(currentItem.ToString(0, currentItem.Length - 1));
                    currentItem.Clear();
                }
            }

            if (inDoubleQuote || inSingleQuote || escaping || currentItem.Length == 0)
            {
                return false;
            }
            rawItems.Add(currentItem.ToString());

            foreach (string rawItem in rawItems)
            {
                string key;
                string value;
                if (!TrySplitKeyValueLine(rawItem, out key, out value))
                {
                    return false;
                }

                string parsedKey = string.Empty;
                string trimmedKey = key.Trim();
                if ((trimmedKey.StartsWith("\"", StringComparison.Ordinal) || trimmedKey.StartsWith("'", StringComparison.Ordinal))
                    && !TryParseScalarValue(trimmedKey, out parsedKey))
                {
                    return false;
                }
                if (trimmedKey.StartsWith("\"", StringComparison.Ordinal) || trimmedKey.StartsWith("'", StringComparison.Ordinal))
                {
                    key = parsedKey;
                }

                string parsedValue;
                if (string.IsNullOrWhiteSpace(key) || !TryParseScalarValue(value, out parsedValue) || result.ContainsKey(key))
                {
                    return false;
                }
                result.Add(key, parsedValue);
            }

            parsedValues = result;
            return true;
        }

        private static bool TryParseDoubleQuotedString(string rawValue, out string parsedValue)
        {
            parsedValue = string.Empty;
            var builder = new StringBuilder();
            bool escaping = false;
            bool closed = false;

            for (int index = 1; index < rawValue.Length; index++)
            {
                char ch = rawValue[index];
                if (escaping)
                {
                    switch (ch)
                    {
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case '"':
                            builder.Append('"');
                            break;
                        case '\\':
                            builder.Append('\\');
                            break;
                        default:
                            builder.Append(ch);
                            break;
                    }
                    escaping = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escaping = true;
                    continue;
                }

                if (ch == '"')
                {
                    string remainder = rawValue.Substring(index + 1).Trim();
                    if (remainder.Length > 0 && !remainder.StartsWith("#"))
                    {
                        return false;
                    }

                    parsedValue = builder.ToString();
                    closed = true;
                    break;
                }

                builder.Append(ch);
            }

            return closed && !escaping;
        }

        private static string StripInlineComment(string value)
        {
            bool inDoubleQuote = false;
            bool inSingleQuote = false;
            bool escaping = false;

            for (int index = 0; index < value.Length; index++)
            {
                char ch = value[index];
                if (escaping)
                {
                    escaping = false;
                    continue;
                }

                if (ch == '\\' && inDoubleQuote)
                {
                    escaping = true;
                    continue;
                }

                if (ch == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (ch == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    continue;
                }

                if (ch == '#' && !inDoubleQuote && !inSingleQuote)
                {
                    return value.Substring(0, index);
                }
            }

            return value;
        }

        private static string[] SplitLines(string text)
        {
            return (text ?? string.Empty).Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        private static string ToTomlString(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }

            var builder = new StringBuilder(value.Length + 2);
            builder.Append('"');
            foreach (char ch in value)
            {
                switch (ch)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        builder.Append(ch);
                        break;
                }
            }
            builder.Append('"');
            return builder.ToString();
        }

        private static string ToTomlStringArray(IEnumerable<string> values)
        {
            return "[" + string.Join(", ", (values ?? Enumerable.Empty<string>()).Select(ToTomlString)) + "]";
        }

        public static string FormatStringMap(IDictionary<string, string> values)
        {
            return string.Join(";", (values ?? new Dictionary<string, string>()).Select(item => (item.Key ?? string.Empty) + "=" + (item.Value ?? string.Empty)));
        }

        private static string SerializeStringMap(IDictionary<string, string> values)
        {
            return "{ " + string.Join(", ", (values ?? new Dictionary<string, string>()).Select(item => ToTomlString(item.Key ?? string.Empty) + " = " + ToTomlString(item.Value ?? string.Empty))) + " }";
        }

        private static string SerializePortValue(LaunchInfoData item)
        {
            string value = item.GetFieldValueString("port") ?? string.Empty;
            int portNumber;
            if (Int32.TryParse(value, out portNumber) && portNumber >= 0 && portNumber <= 65535)
            {
                return portNumber.ToString();
            }

            return ToTomlString(value);
        }

        private static string SerializeRunBackgroundValue(LaunchInfoData item)
        {
            string value = item.GetFieldValueString("run_background") ?? string.Empty;
            return SerializeBooleanValue(value);
        }

        private static string SerializeBooleanValue(string value)
        {
            if (string.Equals(value, "1", StringComparison.Ordinal) || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return "true";
            }

            if (string.Equals(value, "0", StringComparison.Ordinal) || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                return "false";
            }

            return ToTomlString(value);
        }

        private static string SerializePositiveIntegerValue(string value, int fallback)
        {
            int parsedValue;
            if (Int32.TryParse(value, out parsedValue) && parsedValue > 0)
            {
                return parsedValue.ToString();
            }

            return fallback.ToString();
        }

        private static void ValidateSystemSettingsForSave(LauncherAppConfig config)
        {
            string hideOneKeyValue = config.GetSystemFieldValueString("hide_one_key") ?? string.Empty;
            if (!IsSupportedBooleanText(hideOneKeyValue))
            {
                throw new LauncherConfigException(AppLocalization.Get("ConfigFile.SystemHideOneKeyMustBeBoolean"));
            }

            string intervalValue = config.GetSystemFieldValueString("chk_interval") ?? string.Empty;
            int parsedSeconds;
            if (!Int32.TryParse(intervalValue, out parsedSeconds) || parsedSeconds <= 0)
            {
                throw new LauncherConfigException(AppLocalization.Get("ConfigFile.SystemChkIntervalMustBePositiveInteger"));
            }

            string languageValue = config.GetSystemFieldValueString("language") ?? string.Empty;
            string normalizedLanguage;
            if (!AppLocalization.TryNormalizeLanguage(languageValue, out normalizedLanguage))
            {
                throw new LauncherConfigException(AppLocalization.Get("ConfigFile.SystemLanguageInvalid"));
            }
        }

        private static bool IsSupportedBooleanText(string value)
        {
            return string.Equals(value, "1", StringComparison.Ordinal)
                || string.Equals(value, "0", StringComparison.Ordinal)
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        }
    }
}
