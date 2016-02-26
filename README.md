# MoreBlocks
SNEdit is a Mod Project for SkyNations Servers with the aim to provide a toolset for admins and moderators to edit the ingame world. 
Contributors (in no particular order): Aerion, Vanto

## Commands
---

### Implemented so far:

//pos <1/2>  
//set <blockID>  
//replace <blockID> <blockID>  
//copy  
//load <filename>  
//rotate <90/180/270>  
//paste  
//posinfo  

## How to Install the Mod?

Until there is a release you can download the repository, and paste the contents of the `_MOD\` folder into your Skynations-Server root directory. After enabling the Mod in your `Server Settings.xml` (example provided) and adjusting a the file `\GameData\Scripts\Scripts.csproj` according to the example file, you can start your server up and use the new commands.

## How do I contribute?

If you want to contribute, take a look at the public Trello board [here](https://trello.com/b/RdQDKn0t/snessentials). If there is something you are interested in helping with, submit a pull request and or contact me to discuss details.

## About Modding
For further information about modding in Sky Nations, go to [Sky Nations Wiki](http://wiki.skynations.net/doku.php?id=modding "Sky Nations Wiki - Modding").  
Do not forget to check out the `SNScriptUtils.cs`, there you can find lots of useful functions making life with Chunks, Systems and the way SkyNations does things a lot easier.