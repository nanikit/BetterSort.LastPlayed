namespace BetterSongList.LastPlayedSort {
  using System;
  using System.Collections.Generic;

  internal class LastPlayedDateComparer : IComparer<IPreviewBeatmapLevel> {
    public LastPlayedDateComparer(Dictionary<string, DateTime> lastPlayedDates) {
      _lastPlayedDates = lastPlayedDates;
    }

    public int Compare(IPreviewBeatmapLevel a, IPreviewBeatmapLevel b) {
      if (_lastPlayedDates == null) {
        return 0;
      }

      if (_lastPlayedDates.TryGetValue(a.levelID, out var lastPlayOfA)) {
        if (_lastPlayedDates.TryGetValue(b.levelID, out var lastPlayOfB)) {
          var descending = lastPlayOfB.CompareTo(lastPlayOfA);
          return descending;
        }
        return -1;
      }
      else {
        if (_lastPlayedDates.TryGetValue(b.levelID, out var _)) {
          return 1;
        }
        return 0;
      }
    }

    /// <summary>
    /// Level id to instant.
    /// </summary>
    private readonly Dictionary<string, DateTime> _lastPlayedDates;
  }
}