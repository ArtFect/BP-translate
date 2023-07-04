# BP-translate

Partial translator for the Japanese Blue Protocol server. It only translates strings that are downloaded from the api (quests, items, mobs...)  

So far it does not work if you use a proxy for Blue Protocol. VPN is fine
 
<img src="https://i.imgur.com/PwC50La.png" alt= “” width="328" height="341">

(In this screenshot, SkyProc was also used to translate the interface)

[If you have any problems or want to help with translations, check out this discord](https://discord.gg/nVfDBy97aK) 

## How does it work?

The program doesn't change game files, doesn't interact with the game memory, doesn't do dll injection

It redirects domain from which the data is downloaded to localhost via the hosts file.  
Then it creates a server at localhost that proxies all requests to the real server, but replaces localisation file with its translated one.

## How to use it?

Follow `Installation and usage` in this repository: https://github.com/digitalstars/BlueProtocol-Translate  
The current repository is used mostly to host code. A more up-to-date translation can be found at the link above

## Known issue

If you kill a program via Task Manager or otherwise, you may have an endless load when starting the game.

To fix this, run the program again and close normally via X.  Or you can go to `C:/Windows/System32/drivers/etc/hosts` file and remove this line `127.0.0.1 masterdata-main.aws.blue-protocol.com`
