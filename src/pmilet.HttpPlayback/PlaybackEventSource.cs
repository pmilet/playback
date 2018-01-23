using pmilet.HttpPlayback.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pmilet.HttpPlayback
{
        [EventSource(Name = "pmilet.HttpPlayback")]
        public class PlaybackEventSource : EventSource
        {
            public static PlaybackEventSource Current = new PlaybackEventSource();
            static PlaybackEventSource()
            {
                // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
                // This problem will be fixed in .NET Framework 4.6.2.
                Task.Run(() => { });
            }

            public PlaybackEventSource() : base() { }

            [Event(1, Level = EventLevel.Informational, Message = "{0}")]
            public void NewPlaybackMessage(string message, string playbackId)
            {
                if (this.IsEnabled())
                {
                    WriteEvent(1, message, playbackId);
                }
            }

            [NonEvent]
            public void PlaybackModeChanged(PlaybackMode newPlaybackMode)
            {
                PlaybackModeChanged("Playback mode changed", newPlaybackMode.ToString());
            }

            [Event(2, Level = EventLevel.Informational, Message = "{0}")]
            private void PlaybackModeChanged(string message, string newPlaybackMode)
            {
                if (this.IsEnabled())
                {
                    WriteEvent(2, message, newPlaybackMode.ToString());
                }
            }

        [NonEvent]
        public void PlaybackRequestContextChanged(string newRequestPlaybackContext)
        {
            PlaybackRequestContextChanged("Playback request context changed", newRequestPlaybackContext);
        }

        [Event(3, Level = EventLevel.Informational, Message = "{0}")]
        private void PlaybackRequestContextChanged(string message, string newRequestPlaybackContext)
        {
            if (this.IsEnabled())
            {
                WriteEvent(3, message, newRequestPlaybackContext.ToString());
            }
        }

        [NonEvent]
        public void PlaybackFakeChanged(string newPlaybackFake)
        {
            PlaybackRequestContextChanged("Playback fake changed", newPlaybackFake);
        }

        [Event(4, Level = EventLevel.Informational, Message = "{0}")]
        private void PlaybackFakeChanged(string message, string newPlaybackFake)
        {
            if (this.IsEnabled())
            {
                WriteEvent(4, message, newPlaybackFake.ToString());
            }
        }

    }
}
