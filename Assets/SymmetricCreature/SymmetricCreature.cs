using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SymmetricCreature
{
        private int maxXvalue;
        private int maxYvalue;
        private int maxZvalue;
        private int maxBalls;
        private const float defaultdensity = 0.2f;
        private System.Random randomx;
        private System.Random randomy;
        private System.Random randomz;
        private float ballsize;

        public SymmetricCreature(Vector3 componentbox, float density = defaultdensity)
        {
            maxXvalue = (int)componentbox.x;
            maxYvalue = (int)componentbox.y;
            maxZvalue = (int)componentbox.z;
            if (density < 1 && density > 0)
            {
                maxBalls = (int)Mathf.Floor(maxXvalue * maxYvalue * maxZvalue * density);
            }
            else
            {
                maxBalls = (int)Mathf.Floor(maxXvalue * maxYvalue * maxZvalue * defaultdensity);
            }
            randomx = new System.Random();
            randomy = new System.Random();
            randomz = new System.Random();
            ballsize = 1;
    }

        //Creates one Half of the Creature as a random Metaball Mesh
        public Metaball createMetaballMesh()
        {
            Metaball metaball = new Metaball();
            for (int i = 0; i < maxBalls; i++)
            {
                float xvalue = (float)randomx.Next(0, maxXvalue);
                float yvalue = (float)randomy.Next(0, maxYvalue);
                float zvalue = (float)randomz.Next(0, maxZvalue);
                Vector3 position = new Vector3(xvalue, yvalue, zvalue);
                metaball.AddBall(ballsize, position);
            }

        return metaball;
        }

        public void setRandoms(System.Random randomx, System.Random randomy, System.Random randomz)
        {
            this.randomx = randomx;
            this.randomy = randomy;
            this.randomz = randomz;
        }

        public void setBallSize(float ballsize)
        {
        this.ballsize = ballsize;
        }

        public int getMaxX()
        {
            return maxXvalue;
        }
        public int getMaxY()
        {   
            return maxYvalue;
        }
        public int getMaxZ()
        {
            return maxZvalue;
        }

}
