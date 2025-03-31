# Rock Climber App Guide
Welcome to Rock Climber, the bouldering path-finding app for Android. New to bouldering? Check out [this](AdditionalInformation/WhatIsIndoorBouldering.md) page.
Upon Launching the app you will be greeted with [4 options](https://github.com/reed2ep/SeniorDesignProject/blob/main/Assignments/AdditionalInformation/HomePage.jpg). 
If this is your first time using the app you should head to the ***Profile page***.
## User Profile
To navigate to the [User Profile](https://github.com/reed2ep/SeniorDesignProject/blob/main/Assignments/AdditionalInformation/UserProfile.jpg) page select the option ***Go to User Profile***.

You will be prompted with 4 options ***Name, Height, Wingspan, Difficulty Level***

**Name:** This is just a your name, It is optional but can be helpful if multiple people use the app on the same device.

**Height:** Your height should be inputed in Feet and Inches. These measurements will be used to optimize a [route](AdditionalInformation/WhatIsARoute.md) best fit for your height. The default is 5 ft.

**Wingspan:** Similar to height, this is in Feet and Inches and is used alongside height to optimize possible moves during the route calculation. 
- If you are unsure of your wingspan a good estimate is your height

**Difficulty Level:** Difficulty level will be used to adjust possible routes as a faster route may be possible but may be too difficult for a beginner. 

After you finish filling out your User Profile hit **Save Profile**. Your profile will be saved for all future use.

## Using The app
After you have your profile set up the app is ready to use. You have 3 available options.

**Saved Paths:** If you already calculated a path you can choose to save it for later. Those paths wil be saved here for future use. 

### Step 1: Select one of the following options
**[Go to Camera:](https://github.com/reed2ep/SeniorDesignProject/blob/main/Assignments/AdditionalInformation/CameraPage.jpg)** Launches camera and allows you to take a live photo of a bouldering problem you would like a path created for.

**[Open Gallery:](https://github.com/reed2ep/SeniorDesignProject/blob/main/Assignments/AdditionalInformation/ImageSelectPage.jpg)** If you would like to take photos for later or do some prep work prior you can select images from your galley instead of using your camera.

### Step 2: [Select a color](https://github.com/reed2ep/SeniorDesignProject/blob/main/Assignments/AdditionalInformation/ColorSelectPage.jpg)
To determine a route, you need to select a color that best displays the route you want.

For example if you would like to take a route with all red holds you would select **RED**

### Step 3: [Selecting a start point and end point for both hands and legs](https://github.com/reed2ep/SeniorDesignProject/blob/main/Assignments/AdditionalInformation/AnnotationPage.jpg)
After you select confirm you will be prompted with a picture showing all the detected holds as well as hold type.

You are presented with various options:

**Select Hold to Edit:** This is used to select a hold. If you want to modify hold 1 you simpily select hold 1.

**Select or Change Hold Type:** Once you have selected a hold above you will have the option to change the hold type. By default all holds are considered jugs. If you'd like to avoid certain holds and favor others, you can do that here.
1. Select the Desired hold by its number on the image.
2. Change the hold type to what you would categorize it as.
3. Selected hold should be automatically updated with new hold type.

**Select Start Hold:** This is your starting point. By default, it goes to the lowest available hold but should be modified to match where the you would like to start with your hands.

**Select End Hold:** This is your destination. By default it will go to the top hold but can be changed here.

**2 Hand Start:** If the route indicates there are 2 starting hand holds instead of one, you can check this box and you will be prompted to select a start hold for both hands. 

**2 Hand leg Start:** If the route indicates there are 2 starting leg holds, you can check this box and you will prompted to select two leg hold for both legs

**2 Hand End:** If the route indicates there are 2 finish holds instead of one, you can check this box and you will be prompted to select a finish hold for both hands.

**1 Hand End:** If the route indicates there is 1 hold finish instead of two, you can check the following option and you will be prompted where the finish hold is for one hand.

**WingSpan:** If you have already selected wingspan in your user profile and don't want to change(Default wingspan in user-profile) you can choose just next but if you want to change it you have the option given. 

### Step 4:[PathFinding]

1. Once you are out of your config page, you will be sent to the next page, which will show you the results according to the configuration you have made.
2. On this page you will be given five  options (Previous, Next, View Final Path, Back, SavePath )

Explanation for the function of each button

1. Previous Button: This button is basically used once you have your final path but you are stuck at one point you can use the previous button to go to the point where you are stuck and start the path from there again
2. Next Button: This button is used if you want a step-by-step process of reaching the final destination.
3. View Final Path Button: This button is basically used if you want to skip the step-by-step process given by the next button and just want the whole path in one go.
4. Back Button: This button is used to go to the previous pages if you want to make any changes
5. SavePath Button: This button is used to save all of your paths as this can help you to revisit the same path easily instead of doing the same process again and again for the same path.

### Help Button:
This button is used to help users who are having problem understanding how to communicate with the app.
