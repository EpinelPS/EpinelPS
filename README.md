# EpinelPS

---

<div align="center">

[![GitHub issues](https://img.shields.io/github/issues/EpinelPS/EpinelPS?style=flat-square)](https://github.com/EpinelPS/EpinelPS/issues)
[![GitHub pr](https://img.shields.io/github/issues-pr/EpinelPS/EpinelPS?style=flat-square)](https://github.com/EpinelPS/EpinelPS/pulls)
[![GitHub](https://img.shields.io/github/license/EpinelPS/EpinelPS?style=flat-square)](https://github.com/EpinelPS/EpinelPS/blob/main/LICENSE)
![GitHub release (with filter)](https://img.shields.io/github/downloads-pre/EpinelPS/EpinelPS/latest/total?style=flat-square)
![GitHub Repo stars](https://img.shields.io/github/stars/EpinelPS/EpinelPS?style=flat-square)
[![Discord](https://img.shields.io/discord/1261717212448952450?style=flat-square)](https://discord.gg/Ztt6Y9vQjF)

</div>
Private/local server for a 2d anime rpg game. The goal of this project is to replicate the functionality of the official server.

Discord server: https://discord.gg/Ztt6Y9vQjF

For running the game on Linux with EpinelPS, please use https://github.com/EpinelPS/EpinelPSLauncher


> [!CAUTION]
> Please note this GitHub repository (https://github.com/EpinelPS/EpinelPS/) is the only official source for EpinelPS. **If you bought it from someone, you got scammed. Do not download EpinelPS from other sources.** Download link: https://nightly.link/EpinelPS/EpinelPS/workflows/dotnet-desktop/main/Server%20and%20Server%20selector.zip 
[Download link (Linux)](https://nightly.link/EpinelPS/EpinelPS/workflows/dotnet-desktop/main/EpinelPS_linux_x64.zip)

## Usage
Download the [GitHub actions build](https://nightly.link/EpinelPS/EpinelPS/workflows/dotnet-desktop/main/Server%20and%20Server%20selector.zip), and run ServerSelector.Desktop.exe as administrator (to modify DNS hosts file and install a CA cert). Make sure to close the game and launcher first. Select Local server, and then click save. After that, start EpinelPS.exe to start the actual server. You should be able to register an new account in the launcher (you can enter any email verification code). To access the admin panel, go to https://127.0.0.1/admin/ and sign in. You can skip stages and add all characters using that link for example. The first created user account is an admin account.

For more information, please join our [Discord Server](https://discord.gg/Ztt6Y9vQjF)



> [!Note]
> Before updating the game, make sure to switch back to the official server to ensure that the game is properly patched.

### Unlocking counsel logs

In the admin panel, click **Users → Modify → Characters → Unlock All Counsel Logs**. This updates only the selected account's owned characters that have counsel entries in the loaded static data. Exit the game before using it and sign in again afterward to refresh cached progress.

The same operation is available as `unlock-all-counsel-logs` in the server CLI after selecting a user. It adds missing counsel IDs and raises the counsel count when necessary; repeated use is safe. Existing bond levels, experience, favorites, daily availability, and reward-claim status are preserved. It does not grant collection rewards, add characters, or automatically unlock characters obtained later; run it again after obtaining them. No static data pack or `gamecommon.json` changes are needed.

## What is implemented or missing?
See the todo list at https://github.com/orgs/EpinelPS/projects/1 and https://github.com/EpinelPS/EpinelPS/issues
