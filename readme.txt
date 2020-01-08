NATHALIE RAFFRAY
260682940

When the shoppers are flyered, their bodies turn red to distinguish them.

If the advertiser is already following a shopper, if another shopper is detected within euclidean distance of s, it will simply ignore the detected shopper. 

When the shopper goes within the shop, I assume the advertiser stops following it (since the way I've implemented my shops, the advertiser cannot follow inside). 

It wasn't clear in the instructions how to implement the details listed above so I took the initiative to just do it that way. 

You can change k, p, r, s, shopper spawn rate (the number corresponds to how many seconds pass between instantiating one shopper), and total number of advertisers within the "Board Manager" script component in the "Floor" game object at the top of the hierarchy. At the beginning these are all set to 0 except for shopper spawn rate and k. 

When a shopper goes in a shop, they are temporarily beheaded and their sphere head floats in mid air. :)

Steering forces used:

For the shopper I used Collision Avoidance as well as Seek (seeking either chair, shop, or the right end of mall).

For the advertiser I used Separation, a further repulsion velocity (for advertiser against advertiser), wander and Follow the Leader.
I also use a further repulsion velocity that makes sure the advertisers stay within the bounds of the mall. It is a preventive measure. If the advertiser gets too close to the top, bottom, left or right bounds, this velocity is applied. 

If you pause my game, and then unpause it, some weird null references will occur, which will make the shoppers not detect the chairs. I didn't know how to fix this. When the game is in playmode this doesn't happen. So if you need to pause the game, make sure to play it over again rather than unpause. Sorry about this. 