using System;
using UnityEngine;

namespace Hai.Project12.HaiSystems.Supporting
{
    public static class H12Debug
    {
        public static string ColorAsStartTag(Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
        }

        public static void LogError(string message, LogTag logTag = LogTag.Project12)
        {
            Debug.unityLogger.LogError("",FormatMessage(message, logTag, MessageType.Error));
        }
        public static void LogError(Exception message, LogTag logTag = LogTag.Project12)
        {
            Debug.unityLogger.LogError("", FormatMessage($"{message.Message} {message.StackTrace}", logTag, MessageType.Error));
        }

        public static void LogWarning(string message, LogTag logTag = LogTag.Project12)
        {
            LogInternal(message, logTag, MessageType.Warning);
        }

        public static void Log(string message, LogTag logTag = LogTag.Project12)
        {
            LogInternal(message, logTag, MessageType.Info);
        }

        public static void LogInternal(string message, LogTag logTag, MessageType messageType)
        {
            Debug.unityLogger.Log(FormatMessage(message, logTag, messageType));
        }

        public static string FormatMessage(string message, LogTag logTag, MessageType messageType)
        {
            // Retrieve colors for the tag and message type
            string logTagColor = GetTagColor(logTag);
            string messageTypeColor = GetMessageTypeColor(messageType);

            // Format the message with proper syntax
            return $"<color=#242424>[<color={logTagColor}>{logTag}</color>]</color> <color={messageTypeColor}>{message}</color>";
        }

        public static string GetTagColor(LogTag logTag)
        {
            return logTag switch
            {
                // LogTag.Voice => "#FF69B4",          // Hot Pink
                LogTag.ListenNetworking => "#1E90FF",    // Dodger Blue
                LogTag.SteamNetworking => "#32CD32",            // Lime Green
                // LogTag.Core => "#FFD700",          // Gold
                // LogTag.Event => "#FF4500",         // Orange Red
                LogTag.Project12 => "#9370DB",        // Medium Purple
                // LogTag.Device => "#00CED1",        // Dark Turquoise
                LogTag.Vixxy => "#8B0000",        // Dark Red
                LogTag.VixxyNetworking => "#8B0000",        // Dark Red
                // LogTag.Input => "#808000",         // Olive
                // LogTag.Gizmo => "#FF6347",         // Tomato
                // LogTag.Scene => "#4682B4",         // Steel Blue
                _ => "#FFFFFF"                     // Default White
            };
        }

        public static string GetMessageTypeColor(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Error => "#FF0000",    // Red for errors
                MessageType.Warning => "#FFA500", // Orange for warnings
                MessageType.Info => "#FFFFFF",    // Green for logs
                _ => "#FFFFFF"                    // Default White
            };
        }

        public static string FormatLogMessage(LogTag logTag, MessageType messageType, string message)
        {
            string tagColor = GetTagColor(logTag);
            string messageTypeColor = GetMessageTypeColor(messageType);

            // Apply colors
            string formattedTag = $"<color=#808080>[{logTag}]</color>"; // Grey color for tag brackets
            string formattedMessage = $"<color={tagColor}>{message}</color>"; // Tag color for message text

            return $"{formattedTag} <color=#FFFFFF>{messageTypeColor}: {formattedMessage}</color>";
        }

        public enum LogTag
        {
            Project12,
            ListenNetworking,
            SteamNetworking,
            Vixxy,
            VixxyNetworking,
        }

        public enum MessageType
        {
            Info,
            Warning,
            Error
        }
    }

}
