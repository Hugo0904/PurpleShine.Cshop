﻿using Slack.Webhooks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MegaMedia.Logging.Slack
{
    /// <summary>
    /// Slack群組管理
    /// </summary>
    public class SlackUtil
    {
        #region Singleton Pattern
        private static SlackUtil _instance;

        static SlackUtil()
        {
            _instance = new SlackUtil();
        }

        public static SlackUtil Manager
        {
            get { return _instance; }
        }

        private SlackUtil()
        {
            //
        }
        #endregion

        private readonly ConcurrentDictionary<string, SlackClient> _slacks = new ConcurrentDictionary<string, SlackClient>();

        public bool CreateSlack(string identify, string webhooksUrl)
        {
            return _slacks.ContainsKey(identify) || _slacks.TryAdd(identify, new SlackClient(webhooksUrl));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identify"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public SlackUtil Emit(string identify, string channel, string message)
        {
            return Emit(identify, new SlackMessage
            {
                Channel = channel,
                Text = message,
                IconEmoji = Emoji.Peach,
                Username = "桃子姐"
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identify"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public SlackUtil Emit(string identify, SlackMessage message)
        {
            return Emit(identify, message, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identify"></param>
        /// <param name="message"></param>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public SlackUtil Emit(string identify, SlackMessage message, SlackAttachment attachment)
        {
            if (_slacks.TryGetValue(identify, out SlackClient client))
            {
                if (attachment != null)
                {
                    message.Attachments = new List<SlackAttachment> { attachment };
                }
                client.Post(message);
            }
            return this;
        }
    }
}