using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace Multi_ESP
{
    internal class Renderer : Overlay
    {
        private float yOffset = 20;
        //render vals
        public Vector2 screeenSize = new Vector2(1920, 1080);

        //enteties copy
        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        //gui elements
        private bool enableEsp = true;
        private bool enableBones = true;
        private bool enableName = true;
        private bool enableVisibilityCheck = true;

        private float boneThickness = 4;

        private Vector4 enemyColor = new Vector4(1, 0, 0, 1); // red
        private Vector4 teamColor = new Vector4(0, 1, 0, 1); // green
        private Vector4 barColor = new Vector4(0, 1, 0, 1);//green
        private Vector4 nameColor = new Vector4(1,1,1,1); //white
        private Vector4 hiddenColor = new Vector4(0,0,0,1); //black

        //draw list
        ImDrawListPtr drawList;

        protected override void Render()
        {
            //imgui menu
            ImGui.Begin("basic esp");

            ImGui.Checkbox("eneble esp", ref enableEsp);
            if (enableEsp)
            {
                /*ImGui.Checkbox("bones", ref enableBones);
                if (enableBones)
                {
                    ImGui.SliderFloat("bone thickness", ref boneThickness, 4, 500);
                }*/

                ImGui.Checkbox("enable visibility check", ref enableVisibilityCheck);
                ImGui.Checkbox("enable name", ref enableName);

                //team color
                if (ImGui.CollapsingHeader("team color"))
                {
                    ImGui.ColorPicker4("##teamcolor", ref teamColor);
                }

                // enemy color
                if (ImGui.CollapsingHeader("enemy color"))
                {
                    ImGui.ColorPicker4("##enemycolor", ref enemyColor);
                }

                //hp bar color
                if (ImGui.CollapsingHeader("hp bar color"))
                {
                    ImGui.ColorPicker4("##barColor", ref barColor);
                }

                //name color
                if (ImGui.CollapsingHeader("name color") && enableName)
                {
                    ImGui.ColorPicker4("##namecolor", ref nameColor);
                }
            }
            

            //behind the wall color
            if (ImGui.CollapsingHeader("behind the color") && enableVisibilityCheck)
            {
                ImGui.ColorPicker4("##inviscolor", ref hiddenColor);
            }

            // draw overlay
            DrawOverlay(screeenSize);
            drawList = ImGui.GetWindowDrawList();

            //drawsuff
            if (enableEsp)
            {
                foreach(Entity entity in entities)
                {
                    //check if entity in screen
                    if (EntityOnSceen(entity))
                    {
                        //draw methods (all)
                        DrawHealthBar(entity);
                        DrawBox(entity);
                        DrawLine(entity);
                        DrawName(entity);
                        ScopedCheck(entity);
                        //DrawBones(entity);
                    }

                } 
            }

        }

        //check position
        bool EntityOnSceen(Entity entity)
        {
            if(entity.position2d.X > 0 && entity.position2d.X < screeenSize.X && entity.position2d.Y>0 && entity.position2d.Y < screeenSize.Y)
            {
                return true;
            }
            return false;
        }


        // drawing methods

        private void DrawBox(Entity entity)
        {
            // calc box height
            float entityHeight = entity.position2d.Y - entity.viewPosition2D.Y;

            //calc box dimensions
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 4, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.viewPosition2D.X + entityHeight / 4, entity.viewPosition2D.Y + entityHeight);

            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            if (enableVisibilityCheck && localPlayer.team != entity.team)
            {
                boxColor = entity.spotted ? boxColor : hiddenColor;
            }

            // Draw rectangle
            drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));

            // Calculate center of the top side of the rectangle
            Vector2 circleCenter = new Vector2((rectTop.X + rectBottom.X) / 2, rectTop.Y);

            // Calculate radius of the circle (half of the height of the rectangle)
            float circleRadius = entityHeight / 8.5f;

            // hidden check
            

            // Draw circle
            drawList.AddCircle(circleCenter, circleRadius, ImGui.ColorConvertFloat4ToU32(boxColor));
        }

        private void DrawLine(Entity entity)
        {
            Vector4 lineColor = localPlayer.team == entity.team ? teamColor : enemyColor;
            if (enableVisibilityCheck && localPlayer.team != entity.team)
            {
                lineColor = entity.spotted ? lineColor : hiddenColor;
            }

            //draw line
            drawList.AddLine(new Vector2(screeenSize.X / 2, screeenSize.Y), entity.position2d, ImGui.ColorConvertFloat4ToU32(lineColor));

        }

        private void DrawHealthBar(Entity entity)
        {
            //calc the hp bar height
            float entityHeight = entity.position2d.Y - entity.viewPosition2D.Y;

            //calc width
            float boxLeft = entity.viewPosition2D.X - entityHeight / 4 + 0.01f;
            float boxRight = entity.viewPosition2D.X + entityHeight / 4 + 0.01f;

            //calc health bar width and height
            float barPercentWidth = 0.05f; // 5% of box width
            float barHeight = entityHeight * (entity.health / 100f);

            float barPixelWidth = barPercentWidth * (boxRight - boxLeft);

            //calc bar rectangle
            Vector2 barTop = new Vector2(boxLeft - barPixelWidth, entity.position2d.Y - barHeight);
            Vector2 barBottom = new Vector2(boxLeft, entity.position2d.Y);

            //get bar color
            

            //drawing 
            drawList.AddRectFilled(barTop, barBottom, ImGui.ColorConvertFloat4ToU32(barColor));

        }
        private void DrawName(Entity entity)
        {
            if (enableName)
            {
                Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.position2d.Y - yOffset);
                drawList.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}");
            }
            
        }
        private void ScopedCheck(Entity entity)
        {
            Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.position2d.Y + yOffset);
            if (entity.scoped)
            {
                drawList.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"SCOPPED");
            }
        }
        // bones draw methods
        private void DrawBones(Entity entity)
        {
            // get ether team or enemy colorr depending on the team
            uint uintColor = localPlayer.team == entity.team ? ImGui.ColorConvertFloat4ToU32(teamColor) : ImGui.ColorConvertFloat4ToU32(enemyColor);
            float currentBoneThickness = boneThickness / entity.distance;
            //draw lines between bones
            drawList.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);

            drawList.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
            drawList.AddCircleFilled(entity.bones2d[2], 3 + currentBoneThickness, uintColor);
        }

        //transfer entity methods

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);

        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        public Entity GetLocalPlayer()
        {
            lock (entityLock){
                return localPlayer;
            }
        }

        // draw overlay
        void DrawOverlay(Vector2 screenSize)
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0,0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );

        }

    }
}
