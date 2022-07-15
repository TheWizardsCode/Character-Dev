# Pedestrians

Using a NavMesh it is really simple to create a pedestrian system. You simply need to setup areas that you want your characters to avoid and areas you want them to use. Then use the navmesh as normal.

## Example Scene: `Scenes/Crawds/Pedestrians`

In this scene there is a simple city-like area and a number of Pedestrians. When you hit run the pedestrians will start wandering randomly around the city.

The city consists of Roads (pedestrians avoid), Pavements (pedestrians prefer and Crossings (pedestrians will use). As you watch them you will see they take appropriate paths around the city.