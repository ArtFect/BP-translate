## BP-translate

Partial translator for the Japanese Blue Protocol server. It only translates strings that are downloaded from the api (quests, items, mobs...). 

So far the translation is only done with Google Translate, but maybe later there will be a manual translation if there are volunteers

[Project Discord](https://discord.gg/YhKkzdduC)

Program doesn't change game files, doesn't interact with the game memory, doesn't do dll injection

### How does it work?

The program redirects domain from which the data is downloaded to localhost via the hosts file

Then it creates a server at localhost that proxies all requests to the real server, but replaces localization file with its translated one

### How to use it?

1. Download the program archive from [releases](https://github.com/ArtFect/BP-translate/releases) and unzip it
2. Run BP-translate.exe
3. Run the game


BP-translate will close automatically after the localisation file has been issued