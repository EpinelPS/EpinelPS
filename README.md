# EpinelPS

---

<div align="center">

[![GitHub issues](https://img.shields.io/github/issues/MishaProductions/nikke-server?style=flat-square)](https://github.com/MishaProductions/nikke-server/issues)
[![GitHub pr](https://img.shields.io/github/issues-pr/MishaProductions/nikke-server?style=flat-square)](https://github.com/MishaProductions/nikke-server/pulls)
[![GitHub](https://img.shields.io/github/license/MishaProductions/nikke-server?style=flat-square)](https://github.com/MishaProductions/nikke-server/blob/main/LICENSE)
![GitHub release (with filter)](https://img.shields.io/github/downloads-pre/MishaProductions/nikke-server/latest/total?style=flat-square)
![GitHub Repo stars](https://img.shields.io/github/stars/MishaProductions/nikke-server?style=flat-square)
[![Discord](https://img.shields.io/discord/1261717212448952450?style=flat-square)](https://discord.gg/Ztt6Y9vQjF)

</div>
Private/local server for another anime game. Discord server: https://discord.gg/Ztt6Y9vQjF


> [!WARNING]
> This project is in a very early state so many functions in the game do not work.

## Usage
Download the latest release/GitHub actions build, and run ServerSelector.Desktop.exe as administrator (to modify DNS hosts file and install a CA cert). Make sure to close the game and launcher first. Select Local server, and then click save. After that, start EpinelPS.exe to start the actual server.
<br>
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/b01194ef-aec5-4de9-b982-1253757655f8" width="192" height="108">
<br>
You should be able to register an new account in the launcher (you can enter any email verification code).

To access the admin panel, go to https://127.0.0.1/admin/ and sign in. Note that IsAdmin needs to be true for the user account. Note that this interface does not have anything yet.

To skip stages, a basic command line interface is implemented.


## TODO: 
 - [X] Campaign (Normal, Hard, Lost items, Rewards)
 - [X] Lobby
 - [X] Save team info
 - [X] Profile UI
 - [X] Open Archives UI
 - [X] Inventory system
 - [X] Character level up
 - [X] Side story
 - [X] Archives
 - [ ] Skill level up
 - [ ] Outpost jukebox
 - [ ] Admin panel
 - [ ] Test hard stage support
 - [ ] Event system
 - [ ] Download all game assets ahead of time
 - [ ] Basic friend list support
 - [ ] Aegis Diver minigame, MOG minigame, etc
 - [ ] Outpost claim rewards
 - [ ] Daily, weekly missions etc
 - [ ] Lost sector
 - [ ] Custom launcher
 - [ ] Limit temporary participation
 
## What is not working:
 - Collecting items in campaign
 - Events
 - Skill upgrade, limit break
 - Mission reward, daily/weekly missions
 - Side quests
 - Lots of things in the outpost
 - And a lot more...