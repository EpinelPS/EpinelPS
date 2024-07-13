# nikke-server

---

<div align="center">

[![GitHub issues](https://img.shields.io/github/issues/MishaProductions/nikke-server?style=flat-square)](https://github.com/MishaProductions/nikke-server/issues)
[![GitHub pr](https://img.shields.io/github/issues-pr/MishaProductions/nikke-server?style=flat-square)](https://github.com/MishaProductions/nikke-server/pulls)
[![GitHub](https://img.shields.io/github/license/MishaProductions/nikke-server?style=flat-square)](https://github.com/MishaProductions/nikke-server/blob/main/LICENSE)
![GitHub release (with filter)](https://img.shields.io/github/downloads-pre/MishaProductions/nikke-server/latest/total?style=flat-square)
![GitHub Repo stars](https://img.shields.io/github/stars/MishaProductions/nikke-server?style=flat-square)
[![Discord](https://img.shields.io/discord/1261717212448952450?style=flat-square)](https://discord.gg/Ztt6Y9vQjF)

</div>
Private/local server for Nikke. NOTE: This project is in a very early state so many features in the game do not work. Discord server: https://discord.gg/Ztt6Y9vQjF

## Usage
Download the latest release/GitHub actions build, and run ServerSelector.Desktop.exe as administrator (to modify DNS hosts file and install a CA cert). Make sure to close the game and launcher first. Select Local server, and then click save. After that, start nksrv.exe to start the actual server.
<br>
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/b01194ef-aec5-4de9-b982-1253757655f8" width="192" height="108">
<br>
You should be able to register an new account in the launcher (you can enter any email verification code).

If the game does not get past the title screen, open an issue and send %appdata%\..\LocalLow\com.proximabeta\NIKKE\player.log file.

Note that this was tested with the latest version (122.8.20c)

## Progress
Stage, character, outpost and story information is saved and works, as well as player nickname.

<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/75330e0d-ddb5-4d29-b7dd-ab6662306494" width="480" height="270">
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/15b5ea93-bcd1-44b7-81b9-a10d053b7af8" width="480" height="270">
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/70ab4668-70b8-4e2c-bf1b-c84974f5e8ee" width="480" height="270">
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/c6a89fd4-9568-48c2-b4f9-d73807d4043e" width="480" height="270">

## TODO: 
 - [X] Normal campaign
 - [X] Lobby
 - [X] Save team info
 - [X] Profile UI
 - [X] Open Archives UI
 - [X] Inventory system
 - [ ] Outpost jukebox / relics saving
 - [ ] Field obtain object
 - [ ] Admin panel
 - [ ] Test hard stage support
 - [ ] Event system
 - [ ] Download all game assets ahead of time
 - [ ] Level up/skill level up support
 - [ ] Basic friend list support / allow using other servers except 127.0.0.1
 - [ ] Aegis Diver minigame, MOG minigame, etc
 - [ ] Outpost claim rewards
 - [ ] Daily, weekly missions etc
 - [ ] Lost sector
 - [ ] Custom launcher
 - [ ] Limit temporary participation
 - [ ] Archives

## Contributing
Server code structure:

nksrv/LobbyServer: The actual server code.

nksrv/IntlServer: Provides Launcher APIs and authentication

nksrv/Protos: Google protobuf definition files

DataFixupUtil: Utility to parse packets from server/client
