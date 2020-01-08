# Steering-Behaviours

Simulated movement of shoppers and advertisers within a mall using steering behaviours

Mall environment with shops on top and bottom, and tables, chairs and planters in between shops. The quantity and position of the tables, chairs and planters varies at each playthrough.

**Shoppers**

Shoppers spawn on the left side of the mall, and move at slightly varying speeds, generally aiming to exit on the right side of the level. With 50% probability, a spawned agent simply traverses the space, despawning once they reach the other side of the mall. The other 50% shop and eat: they should choose a random shop to visit, navigate toward the shop, enter it, pause for about 1s, leave and attempt to find a free seat in the food court. Once successfully “seated” they wait 2-3s, then leave, and head to the right to despawn.

When a shopper goes in a shop, they are temporarily beheaded and their sphere head floats in mid air. :)

*Steering Forces used for Shopper*: I used Collision Avoidance as well as Seek (seeking either chair, shop, or the right end of mall).

**Advertisers**

Advertisers have two primary actions. The first action is to drop an advertisement (flyer), represented by a My Little Pony poster, at their location while outside of a shop. This is done every k seconds with probability p, and has no effect on advertisers, but a shopper who passes over (or within a small radius of) an advertisement consumes it, causing the shopper to instantly pause for about 2s before resuming their motion. During this period they are considered “flyered.” When the shoppers are flyered, their bodies turn red to distinguish them.

A shopper who has been flyered becomes a target for advertisers. Any wandering advertisers within a (Euclidean) range of s of a flyered shopper will attempt to head for that shopper to deliver their sales pitch—a process considered successful if the advertiser is able to get and stay within a distance r of the shopper for more than 4s. Note that this is longer than the time the shopper pauses! An advertiser who successfully delivers 3 sales pitches despawns, and another advertiser is then spawned to replace them. An advertiser who cannot reach (get within distance r of) an intended shopper within 5s will give up and go back to wandering.

A number is indicated above every advertiser to designate the amount of successful sale pitches they have delivered.

*Steering Forces used for Advertiser*: I used Separation, a further repulsion velocity (for advertiser against advertiser), wander and Follow the Leader.I also use a further repulsion velocity that makes sure the advertisers stay within the bounds of the mall. It is a preventive measure. If the advertiser gets too close to the top, bottom, left or right bounds, this velocity is applied. 

**Tinker with the Controls**

You can change the advertising rate k, probability p, sales pitch distance r, observation distance s, shopper spawn rate (the number corresponds to how many seconds pass between instantiating one shopper), and total number of advertisers within the "Board Manager" script component in the "Floor" game object at the top of the hierarchy. At the beginning these are all set to 0 except for shopper spawn rate and k. 

![](name-of-giphy.gif)
