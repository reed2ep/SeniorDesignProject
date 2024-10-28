MAUI SETUP
1. Open Visual Studio 2022 Installer
2. Select "Modify" and scroll down to Desktop & Mobile
3. Select the ".NET Multi-Platform App UI development
4. Download

Clone Repo
1. Open Visual Studio 
2. Select "Clone a repository" then add "https://github.com/reed2ep/SeniorDesignProject"
3. Select a file path and hit clone

Setup Android Emulator
1. Select Tools in the top toolbar
2. Go to Android > Android Device Manager
3. Select New (I used the default config which was a Pixel 5)
4. Hit create 
5. Once its setup you can start the emulator here when debugging in the future

Setup Project
1. Make sure you are in the "Solution View". If you are in Folder View click the item just to the right of the house symbol and select the solution "RockClimber.sln".
2. Right click on RockClimber and select "Set as startup project"
3. next to the start button click the arror next to "Windows Machine" 
4. Go to Android emulators and select your emulator
5. Now when you run the app should be downloaded and automatically run on your emulator.

Setup a new branch
1. Go to view > git repository
2. Right click main and hit fetch to make sure its up to date
3. Right click main again (or where you want to make a branch from) and select "New local branch from"
4. try to name your branch based on if its a feature test or bugfix such that feature/branchname or similar.
5. Note this branch will be local till you push it

Git
1. View > Git changes
2. Here you can commit and push your changes
