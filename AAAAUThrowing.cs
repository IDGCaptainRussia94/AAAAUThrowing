#define ThrowingCompat

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AAAAUThrowing
{

    public class UThrowing : Mod
    {
        public static UThrowing Instance;

        public UThrowing()
        {
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };
        }
                public override void Load()
        {
            Instance = this;
        }

                 public override void Unload()
        {
            Instance = null;
        }
    }

    public class UThrowingProjectile : GlobalProjectile
    {
        public bool thrown = false;
        public override bool InstancePerEntity
        {
            get
            {
                return true;
            }
        }


#if ThrowingCompat
        //throwing compatible, for now
        public override bool PreAI(Projectile projectile)
        {
            if (projectile.Throwing().thrown)
                projectile.thrown = true;
                    return true;
        }
#endif

    }

        public class UThrowingWeapon : GlobalItem
    {
        public bool thrown = false;

        public UThrowingWeapon()
        {
            thrown = false;
        }

        public override bool InstancePerEntity => true;

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            UThrowingWeapon myClone = (UThrowingWeapon)base.Clone(item, itemClone);
            myClone.thrown = thrown;
            return myClone;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {

            UThrowingPlayer thrownPlayer = Main.LocalPlayer.Throwing();
            UThrowingWeapon weapon = item.Throwing();
            if (weapon.thrown)
            {
                // Get the vanilla damage tooltip
                TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.mod == "Terraria");
                if (tt != null)
                {
                    // We want to grab the last word of the tooltip, which is the translated word for 'damage' (depending on what langauge the player is using)
                    // So we split the string by whitespace, and grab the last word from the returned arrays to get the damage word, and the first to get the damage shown in the tooltip
                    string[] splitText = tt.text.Split(' ');
                    string damageValue = splitText.First();
                    string damageWord = splitText.Last();
                    // Change the tooltip text
                    tt.text = damageValue + " (U)Thrown " + damageWord;
                }
                // Get the vanilla crit tooltip
                tt = tooltips.FirstOrDefault(x => x.Name == "CritChance" && x.mod == "Terraria");
                if (tt != null)
                {
                    string[] thetext = tt.text.Split(' ');
                    string newline = "";
                    List<string> valuez = new List<string>();
                    int counter = 0;
                    foreach (string text2 in thetext)
                    {
                        counter += 1;
                        if (counter > 1)
                            valuez.Add(text2 + " ");
                    }
                    int thecrit = ThrowingUtils.DisplayedCritChance(item);
                    string thecrittype = "(U)Thrown ";
                    valuez.Insert(0, thecrit + "% " + thecrittype);
                    foreach (string text3 in valuez)
                    {
                        newline += text3;
                    }
                    tt.text = newline;
                }
            }
        }

        public override bool ConsumeItem(Item item, Player player)
        {
            UThrowingWeapon weapon = item.Throwing();
            UThrowingPlayer thrownPlayer = player.Throwing();
            if (weapon.thrown)
            {
                if (Main.rand.Next(0, 100) < 33 && thrownPlayer.thrownCost33)
                    return false;
                if (Main.rand.Next(0, 100) < 50 && thrownPlayer.thrownCost50)
                    return false;
            }
            return true;
        }

        public override void HoldItem(Item item, Player player)
        {
            UThrowingWeapon weapon = item.Throwing();
            UThrowingPlayer thrownPlayer = player.Throwing();
            if (weapon.thrown)
            {
                thrownPlayer.thrownCrit += item.crit;
            }

        }


        public override bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            UThrowingWeapon weapon = item.Throwing();
            UThrowingPlayer thrownPlayer = player.Throwing();
            if (weapon.thrown)
            {
                speedX *= thrownPlayer.thrownVelocity;
                speedY *= thrownPlayer.thrownVelocity;
            }
            return true;
        }

        public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float mult, ref float flat)
        {
            UThrowingWeapon weapon = item.Throwing();
            UThrowingPlayer thrownPlayer = player.Throwing();
            if (weapon.thrown)
            {
                mult = thrownPlayer.thrownDamage;
            }
        }

    }

    public static class ThrowingUtils
    {
        public static float thrownDamage(this Player player)
        {
            return player.GetModPlayer<UThrowingPlayer>().thrownDamage;
        }
        public static float thrownVelocity(this Player player)
        {
            return player.GetModPlayer<UThrowingPlayer>().thrownVelocity;
        }
        public static int thrownCrit(this Player player)
        {
            return player.GetModPlayer<UThrowingPlayer>().thrownCrit;
        }
        public static bool thrownCost33(this Player player)
        {
            return player.GetModPlayer<UThrowingPlayer>().thrownCost33;
        }
        public static bool thrownCost50(this Player player)
        {
            return player.GetModPlayer<UThrowingPlayer>().thrownCost50;
        }
        public static bool thrown(this Item item)
        {
            if (item.modItem == null)
                return false;
            return item.GetGlobalItem<UThrowingWeapon>().thrown;
        }
        public static bool thrown(this Projectile projectile)
        {
            if (projectile.modProjectile == null)
                return false;
            return projectile.GetGlobalProjectile<UThrowingProjectile>().thrown;
        }







        public static UThrowingPlayer Throwing(this Player player)
        {
            return player.GetModPlayer<UThrowingPlayer>();
        }
        public static UThrowingWeapon Throwing(this Item item)
        {
            return item.GetGlobalItem<UThrowingWeapon>();
        }
        public static UThrowingProjectile Throwing(this Projectile projectile)
        {
            return projectile.GetGlobalProjectile<UThrowingProjectile>();
        }
        public static int DisplayedCritChance(Item item)
        {
            if ((Main.LocalPlayer.HeldItem.type != item.type))
            {
                return Main.LocalPlayer.Throwing().thrownCrit + item.crit;
            }
            else
            {
                return (Main.LocalPlayer.Throwing().thrownCrit - (Main.LocalPlayer.HeldItem.crit)) + item.crit;
            }
        }
    }

        public class UThrowingPlayer : ModPlayer
    {
    public bool thrownCost33 = false;
    public bool thrownCost50 = false;

        public float thrownVelocity = 1f;
        public float thrownDamage = 1f;
        public int thrownCrit = 4;

        public override void ResetEffects()
        {
            thrownCost33 = false;
            thrownCost50 = false;
            thrownVelocity = 1f;
            thrownDamage = 1f;
            thrownCrit = 4; //Default starting crit value
        }


#if ThrowingCompat
        //throwing compatible, for now
        public override void PostUpdateEquips()
        {
            thrownCrit += player.thrownCrit-4;
            thrownDamage += player.thrownDamage-1f;
            if (player.thrownCost33==true)
            thrownCost33 = true;
            if (player.thrownCost50 == true)
                thrownCost50 = true;
            player.thrownVelocity += thrownVelocity-1f;
        }
#endif

    }

    public class UThrowingNPCs : GlobalNPC
    {

        public override bool InstancePerEntity
        {
            get
            {
                return true;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            UThrowingProjectile weapon = projectile.Throwing();
            UThrowingPlayer thrownPlayer = Main.player[projectile.owner].Throwing();
            if (projectile.owner > -1 && projectile.owner < 255)
            {
                if (weapon.thrown)
                {
                    //npc.AddBuff(BuffID.OnFire,10);
                    crit = false;
                    if (Main.rand.Next(0, 100) < thrownPlayer.thrownCrit)
                    {
                        crit = true;
                        //npc.AddBuff(BuffID.CursedInferno, 60);
                    }
                }
            }
        }
    }
}