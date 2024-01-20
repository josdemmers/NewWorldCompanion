using Microsoft.Extensions.Logging;
using NewWorldCompanion.Constants;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Helpers;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NewWorldCompanion.Services
{
    public class NewWorldDataStore : INewWorldDataStore
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        private List<MasterItemDefinitionsJson> _masterItemDefinitionsJson = new List<MasterItemDefinitionsJson>();
        private List<MasterItemDefinitionsJson> _masterItemDefinitionsJson_Named = new List<MasterItemDefinitionsJson>();
        private List<CraftingRecipeJson> _craftingRecipesJson = new List<CraftingRecipeJson>();
        private List<HouseItemsJson> _houseItemsJson = new List<HouseItemsJson>();
        private ConcurrentDictionary<string, string> _itemDefinitionsLocalisation = new ConcurrentDictionary<string, string>(50, 16500);
        private ConcurrentDictionary<string, string> _namedItemDefinitionsLocalisation = new ConcurrentDictionary<string, string>(50, 3500);

        private bool _available = false;

        private string _loadStatusItemDefinitions = string.Empty;
        private string _loadStatusCraftingRecipes = string.Empty;
        private string _loadStatusHouseItems = string.Empty;
        private string _loadStatusLocalisation = string.Empty;
        private string _loadStatusLocalisationNamed = string.Empty;


        // Start of Constructor region

        #region Constructor

        public NewWorldDataStore(IEventAggregator eventAggregator, ILogger<NewWorldDataStore> logger)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init logger
            _logger = logger;

            // Init store data
            Task.Run(() => UpdateStoreData());
        }

        #endregion

        // Start of Properties region

        #region Properties

        public bool Available { get => _available; set => _available = value; }
        public string LoadStatusItemDefinitions { get => _loadStatusItemDefinitions; set => _loadStatusItemDefinitions = value; }
        public string LoadStatusCraftingRecipes { get => _loadStatusCraftingRecipes; set => _loadStatusCraftingRecipes = value; }
        public string LoadStatusHouseItems { get => _loadStatusHouseItems; set => _loadStatusHouseItems = value; }
        public string LoadStatusLocalisation { get => _loadStatusLocalisation; set => _loadStatusLocalisation = value; }
        public string LoadStatusLocalisationNamed { get => _loadStatusLocalisationNamed; set => _loadStatusLocalisationNamed = value; }

        #endregion

        // Start of Methods region

        #region Methods

        public void UpdateStoreData()
        {
            string resourcePath = string.Empty;

            _loadStatusItemDefinitions = $"ItemDefinitions: 0. Loading common items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            _masterItemDefinitionsJson.Clear();
            _masterItemDefinitionsJson_Named.Clear();

            // MasterItemDefinitions Common
            var masterItemDefinitionsJson = new List<MasterItemDefinitionsJson>();
            resourcePath = @".\Data\MasterItemDefinitions_Common.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                }
            }

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading crafting items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // MasterItemDefinitions Crafting
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_Crafting.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                }
            }

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading faction items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // MasterItemDefinitions Faction
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_Faction.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                }
            }

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading loot items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // MasterItemDefinitions Loot
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_Loot.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                }
            }

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading named items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // MasterItemDefinitions Named
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_Named.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                    _masterItemDefinitionsJson_Named.AddRange(masterItemDefinitionsJson);
                }
            }

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading pvp items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // MasterItemDefinitions PVP
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_PVP.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                    _masterItemDefinitionsJson_Named.AddRange(masterItemDefinitionsJson);
                }
            }

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading quest items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // MasterItemDefinitions Quest
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_Quest.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                }
            }
            
            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}";
            _loadStatusCraftingRecipes = $"CraftingRecipes: 0. Loading recipes";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            _craftingRecipesJson.Clear();

            // CraftingRecipe Json (CraftingRecipesArcana)
            var craftingRecipesJson = new List<CraftingRecipeJson>();
            resourcePath = @".\Data\CraftingRecipesArcana.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Arcana";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesArmorer)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesArmorer.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Armorer";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesCooking)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesCooking.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Cooking";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesDungeon)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesDungeon.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Dungeon";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesEngineer)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesEngineer.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Engineer";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesGypKilm)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesGypKilm.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading GypKilm";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesJeweler)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesJeweler.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Jeweler";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesMisc)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesMisc.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Misc";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesRefining)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesRefining.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Refining";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesSeasons)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesSeasons.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}. Loading Seasons";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // CraftingRecipe Json (CraftingRecipesWeapon)
            craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipesWeapon.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.AddRange(craftingRecipesJson);
                }
            }
            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}";
            _loadStatusHouseItems = $"HouseItems: 0. Loading items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // HouseItems Json
            _houseItemsJson.Clear();
            resourcePath = @".\Data\HouseItems.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    _houseItemsJson = JsonSerializer.Deserialize<List<HouseItemsJson>>(stream) ?? new List<HouseItemsJson>();
                }
            }

            _loadStatusHouseItems = $"HouseItems: {_houseItemsJson.Count}";
            _loadStatusLocalisation = $"Localisation: 0. Loading localisations";
            _loadStatusLocalisationNamed = $"Localisation (named): 0. Loading localisations";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            // ItemDefinitionsLocalisation
            UpdateStoreDataLocalisation();

            // Finished initializing data. Inform subscribers.
            Available = true;
            _eventAggregator.GetEvent<NewWorldDataStoreUpdated>().Publish();
        }

        public void UpdateStoreDataLocalisation()
        {
            if (File.Exists(@".\Cache\ItemDefinitionsLocalisation.json") &&
                File.Exists(@".\Cache\ItemDefinitionsLocalisation_Named.json"))
            {
                LoadLocalisationFromCache();

                // Update cache
                Task.Run(() => UpdateLocalisationCache(1, false));
            }
            else
            {
                UpdateLocalisationCache(50, true);
                UpdateStoreDataLocalisation();
            }
        }

        private void LoadLocalisationFromCache()
        {
            _itemDefinitionsLocalisation.Clear();
            string resourcePath = @".\Cache\ItemDefinitionsLocalisation.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    _itemDefinitionsLocalisation = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(stream) ?? new ConcurrentDictionary<string, string>(50, 16500);
                }
            }

            _loadStatusLocalisation = $"Localisation data: {_itemDefinitionsLocalisation.Count}";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);

            _namedItemDefinitionsLocalisation.Clear();
            resourcePath = @".\Cache\ItemDefinitionsLocalisation_Named.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    _namedItemDefinitionsLocalisation = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(stream) ?? new ConcurrentDictionary<string, string>(50, 3500);
                }
            }

            _loadStatusLocalisationNamed = $"Localisation data (named): {_namedItemDefinitionsLocalisation.Count}";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
            Thread.Sleep(50);
        }

        private void UpdateLocalisationCache(int parallelism, bool showProgress)
        {
            // ItemDefinitionsLocalisation - Itemdefinitions
            ConcurrentDictionary<string, string> itemDefinitionsLocalisation = new ConcurrentDictionary<string, string>(parallelism, 16500);
            ConcurrentDictionary<string, string> namedItemDefinitionsLocalisation = new ConcurrentDictionary<string, string>(parallelism, 3500);
            string resourcePath = @".\Data\javelindata_itemdefinitions_master.loc.xml";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    var xml = XDocument.Load(stream);
                    var queryResult = from loc in xml.Descendants()
                                      where loc.Name.LocalName == "string"
                                      select loc;

                    Parallel.ForEach(queryResult, new ParallelOptions { MaxDegreeOfParallelism = parallelism },
                    loc =>
                    {
                        string key = loc.Attribute("key")?.Value ?? string.Empty;
                        string value = loc.Value;

                        // Supported items so far:
                        // MasterItemDefinitions_Common.json
                        // MasterItemDefinitions_Crafting.json
                        // MasterItemDefinitions_Faction.json
                        // MasterItemDefinitions_Loot.json
                        // MasterItemDefinitions_Named.json
                        // MasterItemDefinitions_PVP.json
                        // MasterItemDefinitions_Quest.json
                        if (_masterItemDefinitionsJson.Any(d => d.Name?.Equals("@" + key, StringComparison.OrdinalIgnoreCase) ?? false))
                        {
                            itemDefinitionsLocalisation.TryAdd(key.ToLower(), value);
                        }
                        if (_masterItemDefinitionsJson_Named.Any(d => (d.Name?.Equals("@" + key, StringComparison.OrdinalIgnoreCase) ?? false) && d.ItemClass.Contains("Named")))
                        {
                            namedItemDefinitionsLocalisation.TryAdd(key.ToLower(), value);
                        }

                        _loadStatusLocalisation = $"Localisation data: {itemDefinitionsLocalisation.Count}";
                        _loadStatusLocalisationNamed = $"Localisation data (named): {namedItemDefinitionsLocalisation.Count}";
                        if (showProgress)
                        {
                            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
                        }
                    });
                }
            }

            // ItemDefinitionsLocalisation - Itemdefinitions - Cleanup duplicates
            itemDefinitionsLocalisation.Remove("ArrowBT2_MasterName".ToLower(), out string? _);
            itemDefinitionsLocalisation.Remove("ArrowBT4_MasterName".ToLower(), out string? _);
            itemDefinitionsLocalisation.Remove("ArrowBT5_MasterName".ToLower(), out string? _);
            // ItemDefinitionsLocalisation - Itemdefinitions - Cleanup resource / item conflicts
            // TODO: Remove workaround when named items are separated.
            itemDefinitionsLocalisation.Remove("2hGreatSword_FlintT5_MasterName".ToLower(), out string? _);

            // ItemDefinitionsLocalisation - HouseItems
            resourcePath = @".\Data\javelindata_housingitems.loc.xml";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    var xml = XDocument.Load(stream);
                    var queryResult = from loc in xml.Descendants()
                                      where loc.Name.LocalName == "string"
                                      select loc;

                    Parallel.ForEach(queryResult, new ParallelOptions { MaxDegreeOfParallelism = parallelism },
                    loc =>
                    {
                        string key = loc.Attribute("key")?.Value ?? string.Empty;
                        string value = loc.Value;

                        // Supported items so far:
                        // HouseItems.json
                        if (_houseItemsJson.Any(d => d.Name?.Equals("@" + key, StringComparison.OrdinalIgnoreCase) ?? false))
                        {
                            itemDefinitionsLocalisation.TryAdd(key.ToLower(), value);
                        }

                        _loadStatusLocalisation = $"Localisation data: {itemDefinitionsLocalisation.Count}";
                        if (showProgress)
                        {
                            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
                        }
                    });
                }
            }

            // Save cache - ItemDefinitionsLocalisation.json
            string fileName = @".\Cache\ItemDefinitionsLocalisation.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);
            using (FileStream stream = File.Create(fileName))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                JsonSerializer.Serialize(stream, itemDefinitionsLocalisation, options);
            }

            // Save cache - ItemDefinitionsLocalisation_Named.json
            fileName = @".\Cache\ItemDefinitionsLocalisation_Named.json";
            path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);
            using (FileStream stream = File.Create(fileName))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                JsonSerializer.Serialize(stream, namedItemDefinitionsLocalisation, options);
            }
        }

        public List<CraftingRecipe> GetCraftingRecipes()
        {
            List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();
            foreach (var craftingRecipeJson in _craftingRecipesJson.FindAll(recipe => !string.IsNullOrWhiteSpace(recipe.RequiredAchievementID)))
            {
                string id = craftingRecipeJson.RequiredAchievementID;
                string tradeskill = craftingRecipeJson.Tradeskill;
                string itemId = _masterItemDefinitionsJson.FirstOrDefault(d => d.SalvageAchievement.Equals(craftingRecipeJson.RequiredAchievementID, StringComparison.OrdinalIgnoreCase))?.ItemID ?? string.Empty;
                string localisationId = _masterItemDefinitionsJson.FirstOrDefault(d => d.SalvageAchievement.Equals(craftingRecipeJson.RequiredAchievementID, StringComparison.OrdinalIgnoreCase))?.Name ?? string.Empty;
                string localisation = _itemDefinitionsLocalisation.GetValueOrDefault(localisationId.Trim(new char[] { '@' }).ToLower()) ?? localisationId.Trim(new char[] { '@' });

                if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(tradeskill) &&
                    !string.IsNullOrWhiteSpace(itemId) && !string.IsNullOrWhiteSpace(localisationId) && !string.IsNullOrWhiteSpace(localisation))
                {
                    craftingRecipes.Add(new CraftingRecipe
                    {
                        Id = id,
                        ItemID = itemId,
                        Localisation = localisation,
                        Tradeskill = tradeskill
                    });
                }
            }

            // Workaround for adding MusicSheets as CraftingRecipe
            foreach (var masterItemDefinitionsJson in _masterItemDefinitionsJson)
            {
                string id = masterItemDefinitionsJson.ItemID;
                string tradeskill = masterItemDefinitionsJson.TradingFamily;
                string itemId = masterItemDefinitionsJson.ItemID;
                string localisationId = masterItemDefinitionsJson.Name;
                string localisation = _itemDefinitionsLocalisation.GetValueOrDefault(localisationId.Trim(new char[] { '@' }).ToLower()) ?? localisationId.Trim(new char[] { '@' });

                if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(tradeskill) &&
                    !string.IsNullOrWhiteSpace(itemId) && !string.IsNullOrWhiteSpace(localisationId) && !string.IsNullOrWhiteSpace(localisation) &&
                    tradeskill.Equals(TradeskillConstants.MusicSheets))
                {
                    craftingRecipes.Add(new CraftingRecipe
                    {
                        Id = id,
                        ItemID = itemId,
                        Localisation = localisation,
                        Tradeskill = tradeskill
                    });
                }
            }

            return craftingRecipes;
        }

        public List<MasterItemDefinitionsJson> GetOverlayResources()
        {
            string resourcePath = string.Empty;

            // MasterItemDefinitions Crafting
            var masterItemDefinitionsJson = new List<MasterItemDefinitionsJson>();
            resourcePath = @".\Data\MasterItemDefinitions_Crafting.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                }
            }

            return masterItemDefinitionsJson.FindAll(items => items.TradingFamily.Equals("RawResources") &&
                items.ItemClass.Contains("+") &&
                !items.ItemClass.Contains("WeaponSchematic"));
        }

        public List<NamedItem> GetNamedItems()
        {
            string resourcePath = string.Empty;

            // MasterItemDefinitions Named
            var masterItemDefinitionsJson = new List<MasterItemDefinitionsJson>();
            resourcePath = @".\Data\MasterItemDefinitions_Named.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                }
            }

            List<NamedItem> namedItems = new List<NamedItem>();
            foreach (var masterItem in masterItemDefinitionsJson.FindAll(items => items.ItemClass.Contains("Named")))
            {

                namedItems.Add(new NamedItem
                {
                    ItemClass = masterItem.ItemClass,
                    ItemID = masterItem.ItemID,
                    Localisation = GetNamedItemLocalisation(masterItem.Name),
                    Tier = masterItem.Tier,
                    BindOnEquip = masterItem.BindOnEquip,
                    BindOnPickup = masterItem.BindOnPickup
                });
            }

            return namedItems;
        }

        public bool IsBindOnPickup(string itemName)
        {
            var localisationId = _itemDefinitionsLocalisation.FirstOrDefault(x => x.Value.Replace("\\n", " ").Equals(itemName, StringComparison.OrdinalIgnoreCase)).Key;
            var item = _masterItemDefinitionsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            var houseItem = _houseItemsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.BindOnPickup;
            }
            if (houseItem != null)
            {
                return houseItem.BindOnPickup;
            }
            return true;
        }

        public bool IsNamedItem(string itemName)
        {
            //var localisationId = _itemDefinitionsLocalisation.FirstOrDefault(x => x.Value.Replace("\\n", " ").Equals(itemName, StringComparison.OrdinalIgnoreCase)).Key;
            //var item = _masterItemDefinitionsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));

            // TODO: Remove workaround when there is an alternative overlay for named items.
            var localisationIds = _itemDefinitionsLocalisation.Where(x => x.Value.Replace("\\n", " ").Equals(itemName, StringComparison.OrdinalIgnoreCase));
            if (localisationIds.Count() == 0)
            {
                return false;
            }
            else if(localisationIds.Count() > 1)
            {
                _logger.LogWarning($"Item: {itemName} is not unique. There are {localisationIds.Count()} entries found");
            }

            // Check all similar named items. If one of them is not "named" return false. 
            // Workaround needed for weapon and resource that both share the name "Flint".
            bool uniqueNamedItem = true;
            foreach (var localisationId in localisationIds)
            {
                var item = _masterItemDefinitionsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId.Key}", StringComparison.OrdinalIgnoreCase));
                if (!item?.ItemClass.Contains("Named") ?? true)
                {
                    uniqueNamedItem = false;
                }
            }
            return uniqueNamedItem;
        }

        public string GetItemId(string itemName)
        {
            var localisationId = _itemDefinitionsLocalisation.FirstOrDefault(x => x.Value.Replace("\\n", " ").Equals(itemName, StringComparison.OrdinalIgnoreCase)).Key;
            MasterItemDefinitionsJson? item = _masterItemDefinitionsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            HouseItemsJson? houseItem = _houseItemsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.ItemID;
            }
            if (houseItem != null)
            {
                return houseItem.HouseItemID;
            }
            return string.Empty;
        }

        public ItemDefinition? GetItem(string itemId)
        {
            var item = _masterItemDefinitionsJson.FirstOrDefault(i => i.ItemID.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            var houseItem = _houseItemsJson.FirstOrDefault(i => i.HouseItemID.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item;
            }
            if (houseItem != null)
            {
                return houseItem;
            }
            return null;
        }

        public string GetLevenshteinItemName(string itemName)
        {
            int currentDistance = int.MaxValue;
            string currentItem = string.Empty;

            foreach (var item in _itemDefinitionsLocalisation)
            {
                int distance = LevenshteinDistance.Compute(itemName, item.Value);
                if (distance <= currentDistance)
                {
                    currentDistance = distance;
                    currentItem = item.Value;
                }
                if (currentDistance == 0) break;
            }

            Debug.WriteLine($"Levenshtein. Item: {itemName}, Match: {currentItem}, Distance: {currentDistance}");

            //return currentDistance <= Math.Max(3, itemName.Length) ? currentItem : itemName;
            return currentDistance <= 5 ? currentItem : itemName;
        }

        public string GetItemLocalisation(string itemMasterName)
        {
            return _itemDefinitionsLocalisation.GetValueOrDefault(itemMasterName.Trim(new char[] { '@' }).ToLower()) ?? itemMasterName.Trim(new char[] { '@' });
        }

        public string GetNamedItemLocalisation(string itemMasterName)
        {
            return _namedItemDefinitionsLocalisation.GetValueOrDefault(itemMasterName.Trim(new char[] { '@' }).ToLower()) ?? itemMasterName.Trim(new char[] { '@' });
        }

        public List<CraftingRecipeJson> GetRelatedRecipes(string itemId)
        {
            // Note: The following recipes are ignored:
            // - Empty recipe.ItemID strings because those are all from downgrade recipes.
            // - Armor / weapons because results are random and we have no price data.
            // - Crafting quest recipes.
            return _craftingRecipesJson.FindAll(recipe => 
                !string.IsNullOrWhiteSpace(recipe.ItemID) &&
                !recipe.CraftingCategory.Equals("Armor") &&
                !recipe.CraftingCategory.Equals("CraftingQuestRecipe") &&
                !recipe.CraftingCategory.Equals("MagicStaves") &&
                !recipe.CraftingCategory.Equals("Tools") &&
                !recipe.CraftingCategory.Equals("Weapons") &&
                !recipe.CraftingCategory.StartsWith("Salvage") &&
                !recipe.CraftingCategory.StartsWith("TimelessShards") &&
                (recipe.Ingredient1.Equals(itemId) ||
                (recipe.Ingredient1.Equals(itemId.Substring(0,itemId.Length-2)) && recipe.Type1.Equals("Category_Only")) ||
                recipe.Ingredient2.Equals(itemId) ||
                recipe.Ingredient3.Equals(itemId) ||
                recipe.Ingredient4.Equals(itemId) ||
                recipe.Ingredient5.Equals(itemId) ||
                recipe.Ingredient6.Equals(itemId) ||
                recipe.Ingredient7.Equals(itemId)));
        }

        public CraftingRecipeJson GetCraftingRecipeDetails(string itemId)
        {
            var craftingRecipesJson = new CraftingRecipeJson();
            craftingRecipesJson = _craftingRecipesJson.FirstOrDefault(recipe => recipe.ItemID.ToLower().Equals(itemId), craftingRecipesJson);
            return craftingRecipesJson;
        }

        #endregion

    }
}
