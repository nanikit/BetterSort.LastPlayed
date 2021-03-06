namespace BetterSort.LastPlayed.External {
  using BetterSort.LastPlayed.Sorter;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using IPALogger = IPA.Logging.Logger;

  public interface IPlayedDateRepository {
    void Save(IReadOnlyDictionary<string, DateTime> playDates);

    StoredData? Load();
  }

  internal class PlayedDateRepository : IPlayedDateRepository {
    public PlayedDateRepository(IPALogger logger) {
      _logger = logger;
    }

    public void Save(IReadOnlyDictionary<string, DateTime> playDates) {
      var sorted = new SortedDictionary<string, DateTime>(
        playDates.ToDictionary(x => x.Key, x => x.Value),
        new LastPlayedDateComparer(playDates)
      );
      string json = JsonConvert.SerializeObject(new StoredData() {
        Version = $"{typeof(PlayedDateRepository).Assembly.GetName().Version}",
        LastPlays = sorted,
      }, Formatting.Indented);
      File.WriteAllText(_path, json);
      _logger.Info($"Saved {playDates.Count} records");
    }

    public StoredData? Load() {
      if (!File.Exists(_path)) {
        _logger.Debug($"Try loading but play history is not exist.");
        return null;
      }

      try {
        string json = File.ReadAllText(_path);
        var data = JsonConvert.DeserializeObject<StoredData>(json);
        _logger.Info($"Loaded {data.LastPlays?.Count.ToString() ?? "no"} records");
        return data;
      }
      catch (Exception exception) {
        _logger.Warn(exception);
        return null;
      }
    }

    private readonly string _path = Path.Combine(Environment.CurrentDirectory, "UserData", "LastPlayedDates.json.dat");
    private readonly IPALogger _logger;
  }

  public class ImmigrationRepository : IPlayedDateRepository {
    internal ImmigrationRepository(IPALogger logger, IPlayedDateRepository repository, SongPlayHistoryImporter importer) {
      _logger = logger;
      _repository = repository;
      _importer = importer;
    }

    public StoredData? Load() {
      return _repository.Load() ?? TryImportingSongPlayHistoryData();
    }

    public void Save(IReadOnlyDictionary<string, DateTime> playDates) {
      _repository.Save(playDates);
    }

    private readonly IPALogger _logger;
    private readonly IPlayedDateRepository _repository;
    private readonly SongPlayHistoryImporter _importer;

    private StoredData? TryImportingSongPlayHistoryData() {
      var history = _importer.Load();
      if (history == null) {
        _logger.Debug("Searched SongPlayHistory data but not found. No history restored.");
        return null;
      }

      _logger.Debug($"Imported SongPlayHistory data, total count: {history.Count}");
      return new StoredData() { LastPlays = history };
    }
  }
}
