# nikke-server
Private/local server for Nikke.

## Usage
First, build and run nksrv project.

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

As Nikke encrypts packet data, you also need to replace C:\NIKKE\NIKKE\game\nikke_Data\Plugins\x86_64\sodium.dll. A patched version of this file can be obtained by contacting MishaProductions on Discord.

After doing the following steps, you can register an account in the launcher (enter anything into email verification code section), and play like normal.

## Progress
Currently, stage data is not saved, only story completion is saved. There are also no rewards currently because those are given server side.


## Contributing
You can help by providing information about what rewards are given when a stage is completed, etc. 

Server code structure:

nksrv/LobbyServer: Handles save data.
nksrv/IntlServer: Provides Launcher APIs and authentication
nksrv/Protos: Google protobuf definition files
DataFixupUtil: Utility to parse packets from server/client