# NewWorldCompanion

A companion app for New World, to keep track of all your learned recipes.

## Table of Contents

- [Features](https://github.com/josdemmers/NewWorldCompanion#features)
- [Planned Features](https://github.com/josdemmers/NewWorldCompanion#planned-features)
- [Installation](https://github.com/josdemmers/NewWorldCompanion#installation)
- [Configuration](https://github.com/josdemmers/NewWorldCompanion#configuration)
- [Usage](https://github.com/josdemmers/NewWorldCompanion#Usage)
- [Licensing](https://github.com/josdemmers/NewWorldCompanion#licensing)

## Features

- Keep track of your learned recipes.

## Planned Features

- Add configuration section to make it easier to setup the app for different resolutions.
- Keep track of the storage content of all towns in Aeternum.
- Add timers to keep track of your cooldowns. e.g. elite chest runs, gypsum.
- Add notes for when you want to write something down.
- Add pricing data for items you hover your mouse over.
- Add overlay to show recipe learned status directy in-game.
- Add localisation for items. Currently English only.

## Installation

- Download the latest version from [Releases](https://github.com/josdemmers/NewWorldCompanion/releases)
- Extract files and run NewWorldCompanion.exe
- Updating from an previous version? Make sure to copy your CraftingRecipeProgress.json and Settings.json to keep your current progress.

## Configuration

Note: Default settings are tested running New World with Window Mode: Fullscreen, at 2560x1440.

### Item tooltips

The app uses the tooltip of an item to read the item name. The default values work with a resolution of 2560x1440. To make it work with other configurations you can tweak the following settings.
- Hysteresis thresholds: The lower you set those thresholds the more objects it will detect. This setting is unrelated to your resolution and the default value should work in most cases.
- Area thresholds: The min and max values for this threshold depend on your resolution. It is used to filter out unrelated objects so that only the item tooltip is recognized. The values are correct when you see a single orange outline around the item icon. As shown in the image below. If you do not see an outline start with the lowest min value and the highest max value. Then increase the min value step-by-step, and decrease the max value step-by-step. Do not keep those settings at their lowest/highest values as that will detect many other objects besides the item tooltip.

![Config-1](./readme/readme-config1.png)

### OCR

The threshold values for OCR are used to filter out background noice. Your threshold values are correct if you only see black text on a white background.

![Config-2](./readme/readme-config2.png)

## Usage

todo

## Licensing

MIT

## Thirdparty packages

- [Emgu CV](https://www.emgu.com/wiki/index.php/Main_Page)
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
- [PInvoke](https://github.com/dotnet/pinvoke)
- [Prism](https://github.com/PrismLibrary/Prism)
- [TesserNet](https://github.com/CptWesley/TesserNet)
