using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows.Data;

namespace EDShoppingList
{
    public class ModuleSearchViewModel : ReactiveObject, IProgress<string>
    {
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private Subject<bool> _dataLoaded = new Subject<bool>();
        private Repository _repository = new Repository();
        private ReactiveList<ModuleSearchResponse> _searchResults = new ReactiveList<ModuleSearchResponse>();
        public ICollectionView SearchResults
        {
            get;
        }

        private ReactiveList<ModuleSearchQuery> _searchQueries = new ReactiveList<ModuleSearchQuery>();
        public ReactiveList<ModuleSearchQuery> SearchQueries
        {
            get
            {
                return _searchQueries;
            }
        }

        public bool IsDistanceDisplayed
        {
            get
            {
                return Math.Abs(DistanceToSystem) > double.Epsilon &&
                       !string.IsNullOrWhiteSpace(System);
            }
        }

        private bool _showOnlyStationsWithAllModules;
        public bool ShowOnlyStationsWithAllModules
        {
            get { return _showOnlyStationsWithAllModules; }
            set
            {
                this.RaiseAndSetIfChanged(ref _showOnlyStationsWithAllModules, value);
            }
        }


        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                this.RaiseAndSetIfChanged(ref _status, value);
            }
        }

        private double _distanceToSystem;
        public double DistanceToSystem
        {
            get
            {
                return _distanceToSystem;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _distanceToSystem, value);
                this.RaisePropertyChanged("IsDistanceDisplayed");
            }
        }

        private string _system;
        public string System
        {
            get { return _system; }
            set
            {
                this.RaiseAndSetIfChanged(ref _system, value);
                this.RaisePropertyChanged("IsDistanceDisplayed");
            }
        }

        private ReactiveList<JsonSystem> _systems = new ReactiveList<JsonSystem>();
        public IReactiveDerivedList<string> SystemsNames
        {
            get;
        }

        private ReactiveList<JsonModule> _modules = new ReactiveList<JsonModule>();
        public IReactiveDerivedList<string> Modules
        {
            get;
        }


        public IReactiveCommand<IEnumerable<ModuleSearchResponse>> SearchModuleCommand { get; }
        public IReactiveCommand<object> AddSearchQueryCommand { get; }
        public IReactiveCommand<object> RemoveSearchQueryCommand { get; }

        public ModuleSearchViewModel()
        {
            SystemsNames = _systems.CreateDerivedCollection(s => s.name);
            Modules = _modules.CreateDerivedCollection(s => s.group.name);

            SearchResults = CollectionViewSource.GetDefaultView(_searchResults);
            SearchResults.Filter = _ =>
            {
                if (!ShowOnlyStationsWithAllModules)
                {
                    return true;
                }
                var r = (ModuleSearchResponse)_;
                foreach (var sq in _searchQueries)
                {
                    if (!_searchResults.Any(sr => sr.Station == r.Station &&
                                                  sq.Name == sr.Module))
                    {
                        return false;
                    }
                }
                return true;
            };
            this.WhenAnyValue(vm => vm.ShowOnlyStationsWithAllModules)
                .Subscribe(_ => SearchResults.Refresh());

            RemoveSearchQueryCommand = ReactiveCommand.Create();
            RemoveSearchQueryCommand.Subscribe(c =>
            {
                SearchQueries.Remove((ModuleSearchQuery)c);
            });

            AddSearchQueryCommand = ReactiveCommand.Create();
            AddSearchQueryCommand.Subscribe(_ =>
            {
                SearchQueries.Add(new ModuleSearchQuery());
            });

            var dataLoadedObservable = _repository.OpenAsync(this).ToObservable();
            dataLoadedObservable.ObserveOn(DispatcherScheduler.Current)
                .Subscribe(_ =>
                {
                    _dataLoaded.OnNext(true);

                    _systems.Clear();
                    using (var suppressor = _systems.SuppressChangeNotifications())
                    {
                        foreach (var system in _repository.GetSystems())
                        {
                            _systems.Add(system);
                        }

                    }

                    _modules.Clear();
                    using (var suppressor = _modules.SuppressChangeNotifications())
                    {
                        foreach (var module in _repository.GetModules())
                        {
                            _modules.Add(module);
                        }
                    }

                    Report("Ready");
                });

            SearchModuleCommand = ReactiveCommand.CreateAsyncTask(
                _dataLoaded,
                async _ =>
                {
                    Report("Searching...");
                    _stopWatch.Reset();
                    _stopWatch.Start();
                    return await _repository.SearchModules(SearchQueries, DistanceToSystem, System, this);
                });

            SearchModuleCommand.Subscribe(results =>
            {
                _stopWatch.Stop();
                _searchResults.Clear();
                using (var suppressor = _searchResults.SuppressChangeNotifications())
                {
                    foreach (var r in results)
                    {
                        _searchResults.Add(r);
                    }
                }
                Report($"Done, {_searchResults.Count} results found in {ToReadableString(_stopWatch.Elapsed)}");
            });
        }

        // http://stackoverflow.com/questions/842057/how-do-i-convert-a-timespan-to-a-formatted-string
        private static string ToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}{4}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1} ", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Milliseconds > 0 ? string.Format("{0:0} millisecond{1}", span.Milliseconds, span.Milliseconds == 1 ? string.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }


        public void Report(string value)
        {
            Status = value;
        }
    }
}
