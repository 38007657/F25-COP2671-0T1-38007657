# COP2671FY25 Unity 2D Farming Simulation Game Final Project

## Overview

This is a 2D farming simulation game built in Unity where you manage a small farm, grow crops, and build your agricultural empire. Plant seeds, water your crops, harvest at the right time, and sell your produce to earn coins.

## Game Objective

Start with a modest plot of farmland and basic resources. Your goal is to:
- Cultivate a variety of crops successfully
- Build up your coin reserves through smart farming
- Manage your time efficiently between day and night cycles
- Discover hidden secrets on your farm

## Getting Started

### Starting a New Game

When you first launch the game, you'll see the main menu with several options:
- **New Game**: Start fresh with starting resources and a clean farm
- **Continue**: Resume from your most recent save (if available)
- **Load Game**: Choose from multiple saved games
- **Quit**: Exit the game

Starting a new game gives you:
- 500 coins
- A selection of starter seeds
- An empty farm ready to cultivate

### The Farm Layout

Your farm consists of a grid-based plot of land where you can till soil and plant crops. The farmable area is clearly defined on the map. Navigate around your farm to find the best spots for your crops and look out for special locations.

## Controls

### Movement
- **Arrow Keys**: Move your character around the farm
- Character moves in four directions: up, down, left, right

### Farming Actions
All farming actions are performed by clicking the toolbar buttons at the bottom of the screen with your cursor:

- **Hoe Button**: Till the soil to prepare it for planting
- **Plant Button**: Plant seeds on tilled soil (must have seeds selected)
- **Water Button**: Water your planted crops (required for growth)
- **Harvest Button**: Collect fully grown crops

### Seed Selection
- **S Key**: Toggle the seed selection bar on/off
- **Left Sidebar**: Click on a seed type to select it for planting
- Selected seed will be highlighted in the seed bar
- Close the seed bar to continue playing the game

### Inventory & Menus
- **I Key**: Open/close inventory and shop menu
- **ESC Key**: Pause game and access save/load menu

### Time Controls
- **1 Key**: Normal time speed (1x)
- **2 Key**: Fast time speed (2x)
- **3 Key**: Faster time speed (3x)
- **Tab Key**: Skip to next morning (6 AM)

## How to Play

### Basic Farming Cycle

1. **Select Seeds**
   - Press **S** to open the seed selection bar
   - Click on the seed type you want to plant
   - The selected seed will be highlighted

2. **Prepare the Soil**
   - Move near an empty farmable tile
   - Click the **Hoe button** to till the soil
   - Tilled soil will change appearance

3. **Plant Seeds**
   - Stand near tilled soil
   - Make sure you have seeds selected in the seed bar
   - Click the **Plant button**
   - One seed will be consumed from your inventory

4. **Water Daily**
   - Crops must be watered each day to grow
   - Click the **Water button** when standing near a crop
   - Watered soil will appear darker/wet

5. **Wait for Growth**
   - Most crops grow one stage per day if watered
   - Crops have 4 growth stages: Seed → Sprout → Growing → Harvestable
   - Unwatered crops will wilt after the seed stage

6. **Harvest Crops**
   - When crops reach the final stage, they're ready to harvest
   - Look for visual indicators (particles or color changes)
   - Click the **Harvest button** to collect the crop
   - Harvested items appear as pickups that auto-collect when you walk near them

### Time Management

The game operates on a day/night cycle:

- **6 AM - 6 PM**: Daytime (18 hours)
- **6 PM - 6 AM**: Nighttime (6 hours)
- **Farming Restriction**: You cannot perform farming actions after 6 PM
- Each new day starts at 6 AM (sunrise)

**Time Speed Controls:**
- Use number keys **1**, **2**, or **3** to adjust time speed
- Press **Tab** to skip directly to the next morning
- Plan your farming activities to finish before 6 PM

### Crop Growth System

**Day-Based Crops** (Traditional):
- Advance one growth stage per day at sunrise (6 AM)
- Must be watered the previous day to advance
- Take 3-7 days to reach harvest depending on crop type

**Growth Stages:**
- Stage 0: Seed (just planted)
- Stage 1: Sprout (small plant emerging)
- Stage 2: Growing (plant developing)
- Stage 3: Harvestable (ready to collect)

**Wilting:**
- Crops beyond the seed stage will wilt if not watered
- Wilted crops cannot be recovered
- Use the hoe to remove wilted crops and start over
- Seeds (Stage 0) won't wilt but won't grow without water

### Economy System

**Earning Money:**
- Harvest crops and sell them in the shop
- Different crops have different sell values
- Higher-tier crops generally sell for more
- Look for special opportunities like treasure chests

**Spending Money:**
- Purchase seed packets from the shop
- Each packet contains multiple seeds
- Seed prices vary by crop type
- Plan purchases based on profit margins

**Opening the Shop:**
- Press **I** to open the inventory menu
- Navigate to the **Shop** tab
- **Buy Section**: Purchase seed packets
- **Sell Section**: Sell your harvested crops

### Inventory Management

Press **I** to access your inventory:

**Inventory Tab:**
- View all your seed packets and quantities
- See your harvested crops
- Track what you currently own

**Money Tab:**
- Check your current coin balance

**Shop Tab:**
- **Seeds for Sale**: Browse and purchase new seed packets
- **Sell Crops**: Select crops to sell and confirm transactions
- Use arrow buttons to choose quantity when selling

### Saving and Loading

**Saving Your Game:**
1. Press **ESC** to open the pause menu
2. Click **Save/Load**
3. Click **New Save** button
4. Enter a name for your save
5. Click **Confirm**

**Loading a Game:**
1. From the main menu, click **Load Game**
2. Or press **ESC** → **Save/Load** during gameplay
3. Click the **Load** button on any save slot
4. Your game state will be restored

**Save Information Includes:**
- Current day and time
- Your position on the farm
- All planted and growing crops
- Complete inventory (seeds and harvested items)
- Total money
- Play time

## Special Features

### Treasure Chest
- Contains random coin rewards (50-300 coins) every 3 days
- Look for particle effects indicating availability
- Walk up to chest and press **E** to open

### Easter Eggs
- The game contains hidden secrets
- Explore different areas of your farm
- Try unusual combinations of actions
- Rewards may include bonus coins or special effects