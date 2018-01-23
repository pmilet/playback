using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pmilet.HttpPlayback
{
    public static class PlaybackIdExtension
    {
        public static string Context(this string playbackId)
        {
            return Extract(playbackId,0);
        }

        public static string Name(this string playbackId)
        {
            return Extract(playbackId, 1);
        }

        public static string Version(this string playbackId)
        {
            return Extract(playbackId, 2);
        }

        public static string RequestPath(this string playbackId)
        {
            return Extract(playbackId, 3);
        }

        public static string RequestMethod(this string playbackId)
        {
            return Extract(playbackId, 4);
        }

        public static string HashCode(this string playbackId)
        {
            return Extract(playbackId, 5);
        }

        private static string Extract(string playbackId, int index)
        {
            var arr = playbackId.Split('_');
            if (arr.Length > index)
            {
                return arr[index];
            }
            return string.Empty;
        }


    }
}
