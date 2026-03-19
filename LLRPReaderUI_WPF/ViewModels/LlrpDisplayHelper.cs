using System.Text;

namespace LLRPReaderUI_WPF.ViewModels
{
    /// <summary>
    /// 提供用于在UI中清晰显示LLRP数据的静态辅助方法。
    /// </summary>
    public static class LlrpDisplayHelper
    {
        /// <summary>
        /// 将LLRP UTC微秒时间戳转换为本地化的、人类可读的字符串。
        /// </summary>
        public static string FormatUtcMicroseconds(ulong microseconds)
        {
            try
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                // 1微秒 = 10 Ticks
                var dateTime = epoch.AddTicks((long)microseconds * 10);
                // 转换为本地时间以便查看
                return dateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            }
            catch
            {
                return $"{microseconds} µs (转换失败)";
            }
        }



        /// <summary>
        /// 获取枚举值的名称，如果失败则返回其数字值。
        /// </summary>
        public static string FormatEnum(object? enumValue)
        {
            if (enumValue == null) return "N/A";
            var type = enumValue.GetType();
            if (!type.IsEnum) return enumValue.ToString() ?? "N/A";

            try
            {
                // 显示名称和数字值，更清晰
                return $"{Enum.GetName(type, enumValue)} ({(int)enumValue})";
            }
            catch
            {
                return enumValue.ToString() ?? "N/A";
            }
        }


        public static string FormatHex(byte[] payload, int bytesPerLine = 16)
        {
            if (payload == null || payload.Length == 0) return string.Empty;
            var sb = new StringBuilder();
            for (int i = 0; i < payload.Length; i++)
            {
                if (i % bytesPerLine == 0)
                {
                    if (i > 0) sb.AppendLine();
                    sb.Append(i.ToString("X4")).Append(": ");
                }
                sb.Append(payload[i].ToString("X2")).Append(" ");
            }
            return sb.ToString();
        }


    }
}
