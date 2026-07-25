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
        public List<LaunchInfoData> Items = new List<LaunchInfoData>();
    }


    static class LauncherConfigFile
    {
        private const int MaxExpansionRounds = 50;
        private static readonly Regex PlaceholderRegex = new Regex(@"\{([A-Za-z0-9_\-.]+)\}", RegexOptions.Compiled);
        private static readonly Regex SectionNameRegex = new Regex(@"^[A-Za-z0-9_-]+$", RegexOptions.Compiled);

        public static LauncherAppConfig Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new LauncherAppConfig();
            }

            string rawText = File.ReadAllText(filePath, Encoding.UTF8);
            string expandedText = Expand(rawText ?? string.Empty);
            return Parse(expandedText, false);
        }

        public static LauncherAppConfig LoadRaw(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new LauncherAppConfig();
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

            foreach (LaunchInfoData item in config.Items)
            {
                builder.AppendLine();
                builder.Append('[').Append(item.SectionName).AppendLine("]");
                builder.Append("name = ").AppendLine(ToTomlString(item.Name ?? string.Empty));
                builder.Append("command = ").AppendLine(ToTomlString(item.Command ?? string.Empty));
                builder.Append("stop_command = ").AppendLine(ToTomlString(item.StopCommand ?? string.Empty));
                builder.Append("port = ").AppendLine(SerializePortValue(item));
                builder.Append("requirement_command = ").AppendLine(ToTomlString(item.RequirementCommand ?? string.Empty));
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
                throw new LauncherConfigException("配置对象为空。");
            }

            var usedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (LaunchInfoData item in config.Items)
            {
                string validationMessage;
                if (!TryValidateSectionName(item.SectionName, out validationMessage))
                {
                    throw new LauncherConfigException("配置标识 " + (item.SectionName ?? "<empty>") + " 无效: " + validationMessage);
                }

                if (!usedSections.Add(item.SectionName))
                {
                    throw new LauncherConfigException("配置标识重复: " + item.SectionName);
                }
            }
        }

        public static bool TryValidateSectionName(string sectionName, out string message)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                message = "不能为空。";
                return false;
            }

            if (string.Equals(sectionName, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase))
            {
                message = "不能使用保留名称 system。";
                return false;
            }

            if (!SectionNameRegex.IsMatch(sectionName))
            {
                message = "只能包含字母、数字、下划线和短横线。";
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
                        throw new LauncherConfigException("重复的 section: " + sectionName, lineNumber);
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
                            throw new LauncherConfigException("无效的 section 名称: " + sectionName + "。" + validationMessage, lineNumber);
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
                    throw new LauncherConfigException("配置文件格式错误，需要先声明 section。", lineNumber);
                }

                string key;
                string rawValue;
                if (!TrySplitKeyValueLine(line, out key, out rawValue))
                {
                    throw new LauncherConfigException("配置项格式错误。", lineNumber);
                }

                string value = ParseScalarValue(rawValue, lineNumber);
                string loweredKey = key.Trim().ToLowerInvariant();

                if (string.Equals(currentSection, ProgramGlobalConfig.SystemSectionName, StringComparison.OrdinalIgnoreCase))
                {
                    if (loweredKey == "title")
                    {
                        config.Title = value;
                    }
                    continue;
                }

                try
                {
                    if (preserveRawValues)
                    {
                        currentItem.SetFieldsFromRawString(loweredKey, value);
                    }
                    else
                    {
                        currentItem.SetFieldsFromString(loweredKey, value);
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

            int dotIndex = token.IndexOf('.');
            if (dotIndex <= 0 || dotIndex >= token.Length - 1)
            {
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
                throw new LauncherConfigException("无法解析配置值。", lineNumber);
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
    }
}

