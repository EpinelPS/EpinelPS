# nikke-server
Private/local server for Nikke. NOTE: This project is in a very early state.

## Usage
Download the latest release/GitHub actions build, and run ServerSelector.Desktop.exe as administrator (to modify DNS hosts file and install a CA cert). Make sure to close the game and launcher first. Select Local server, and then click save.

You should be able to register an new account in the launcher (you can enter any email verification code).

If the game does not get past the title screen, open an issue and send %appdata%\..\LocalLow\com.proximabeta\NIKKE\player.log file.
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/b01194ef-aec5-4de9-b982-1253757655f8" width="192" height="108">


## Progress
Stage, character, and story information is saved and works, as well as player nickname.
TODO: reward system (works but does not show in UI), xp system, sim room, outpost, etc
TODO: Create a launcher replacement

<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/75330e0d-ddb5-4d29-b7dd-ab6662306494" width="480" height="270">
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/15b5ea93-bcd1-44b7-81b9-a10d053b7af8" width="480" height="270">
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/70ab4668-70b8-4e2c-bf1b-c84974f5e8ee" width="480" height="270">
<img src="https://github.com/MishaProductions/nikke-server/assets/106913236/c6a89fd4-9568-48c2-b4f9-d73807d4043e" width="480" height="270">


## Contributing
Server code structure:

nksrv/LobbyServer: Handles save data.

nksrv/IntlServer: Provides Launcher APIs and authentication

nksrv/Protos: Google protobuf definition files

DataFixupUtil: Utility to parse packets from server/client

## Manual installation
First, build and run nksrv project. Next, open libsodium-1.0.18-RELEASE/libsodium.sln and build that as well. Ignore the failed tests as the encryption public key was hardcoded.

After that, add the following to your C:\Windows\System32\hosts or /etc/hosts file to use the local server:

```
127.0.0.1 cloud.nikke-kr.com
127.0.0.1 global-lobby.nikke-kr.com
127.0.0.1 aws-na-dr.intlgame.com
127.0.0.1 sg-vas.intlgame.com
127.0.0.1 aws-na.intlgame.com
127.0.0.1 common-web.intlgame.com
127.0.0.1 li-sg.intlgame.com
127.0.0.1 data-aws-na.intlgame.com
255.255.221.21 sentry.io
```

Run generate_ssl_cert.sh in WSL or linux to generate SSL certificates. Make sure to trust myCA.pfx. Append the following (with your CA cert pem that you generated) to Launcher/intl_service/cacert.pem and NIKKE/game/nikke_Data/Plugins/cacert.pem

```
Good SSL CA
===============================
<ca cert>
```
As Nikke encrypts packet data, you also need to replace C:\NIKKE\NIKKE\game\nikke_Data\Plugins\x86_64\sodium.dll from the one you built.

NOTE: Make sure to undo these modifications (especially change back sodium.dll) to play on the offical servers. 

After doing the following steps, you can register an account in the launcher (enter anything into email verification code section), and play like normal.
