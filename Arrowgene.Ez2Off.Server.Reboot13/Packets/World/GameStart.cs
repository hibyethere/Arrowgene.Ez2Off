/*
 * This file is part of Arrowgene.Ez2Off
 *
 * Arrowgene.Ez2Off is a server implementation for the game "Ez2On".
 * Copyright (C) 2017-2018 Sebastian Heinz
 * Copyright (C) 2017-2018 Halgulaea
 * Copyright (C) 2017-2018 David Via
 *
 * Github: https://github.com/Arrowgene/Arrowgene.Ez2Off
 *
 * Arrowgene.Ez2Off is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Arrowgene.Ez2Off is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Arrowgene.Ez2Off. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Threading;
using Arrowgene.Ez2Off.Common.Models;
using Arrowgene.Ez2Off.Server.Client;
using Arrowgene.Ez2Off.Server.Packet;
using Arrowgene.Services.Buffers;
using Arrowgene.Ez2Off.Server.Reboot13.Packets.Builder;

namespace Arrowgene.Ez2Off.Server.Reboot13.Packets.World
{
    public class GameStart : Handler<WorldServer>
    {
        public GameStart(WorldServer server) : base(server)
        {
        }

        public override int Id => 0x0F;//15


        /*
        public float checkAvatar(int avatarId){
            if(avatarId == 277){    // 산타페
                return 1.6f;
            }
            else if(avatarId == 217){   // 글래디스
                return 2.0f;
            }
            return 1.0f;
        }
        */

        /*
        public float checkKey(int keyId, int wideId, Game game){
            if(game.Type == GameType.SinglePlayer){ // 싱글 플레이
                if(keyId != 0 && wideId != 0){  // 해금열쇠 착용 O, 와이드 착용 O
                    return 2.2f;
                }
                else if(keyId == 0 && wideId != 0){ // 해금열쇠 착용 X, 와이드 착용 O
                    return 1.8f;
                }
                else if(keyId != 0 && wideId == 0){ // 해금열쇠 착용 O, 와이드 착용 X
                    return 1.6f;
                }
                // 해금열쇠 착용 X, 와이드 착용 X
                return 2.0f;

            }
            else{ // 멀티 플레이 2.0f
                return 2.0f;
            }
            
        }
        */
        

        public override void Handle(EzClient client, EzPacket packet)
        {
            Game game = new Game();
            game.Name = client.Room.Info.Name;
            game.GroupType = client.Room.Info.GameGroupType;
            game.Type = client.Room.Info.GameType;
            if (!Database.InsertGame(game))
            {
                // Oh uh
            }
            
            
            foreach (EzClient c in client.Room.GetClients())
            {
                c.Player.Playing = true;
                c.Game = game;
                c.Room.Info.Playing = true; // Newly Added for checking playing state
            }

            //float MeasureScale = checkAvatar(client.Inventory.GetAvatarId());
            //float MeasureScale = checkKey(client.Inventory.GetSpecialId(), client.Inventory.GetWideEffectId(), client.Game);

            float MeasureScale;
            Song song = Database.SelectSong(client.Room.Info.SelectedSong);
            
            MeasureScale = 1350 / (song.Bpm * 4.5f); // 150BPM / 4.5배속 / 2.0f 기준 스크롤 속도 = 1350
            // ScrollSpeed = BPM * Speed * MeasureScale
            // -> MeasureScale = ScrollSpeed / (BPM * Speed)


            if(client.Room.Info.SelectedSong == 210){ // Sudden Death
                IBuffer buffer = EzServer.Buffer.Provide();
                if(client.Room.Info.Difficulty == DifficultyType.EZ){ // EZ
                    //Song info
                    //General BYTE / Float
                    buffer.WriteByte(7);//Level
                    buffer.WriteFloat(MeasureScale);//MeasureScale
                    //JudgmentDelta BYTE
                    buffer.WriteByte(8);//Kool
                    buffer.WriteByte(24);//Cool
                    buffer.WriteByte(60);//Good
                    buffer.WriteByte(76);//Miss
                    //GaugeUpDownRate Float
                    buffer.WriteFloat(0.2f);//Cool
                    buffer.WriteFloat(0.1f);//Good
                    buffer.WriteFloat(-1.5f);//Miss
                    buffer.WriteFloat(-4.0f);//Fail
                    buffer.WriteByte(0);
                }
                else if(client.Room.Info.Difficulty == DifficultyType.NM){ // NM
                    //Song info
                    //General BYTE / Float
                    buffer.WriteByte(14);//Level
                    buffer.WriteFloat(MeasureScale);//MeasureScale
                    //JudgmentDelta BYTE
                    buffer.WriteByte(4);//Kool
                    buffer.WriteByte(12);//Cool
                    buffer.WriteByte(30);//Good
                    buffer.WriteByte(38);//Miss
                    //GaugeUpDownRate Float
                    buffer.WriteFloat(0.1f);//Cool
                    buffer.WriteFloat(0.05f);//Good
                    buffer.WriteFloat(-2.5f);//Miss
                    buffer.WriteFloat(-6.5f);//Fail
                    buffer.WriteByte(0);
                }
                else if(client.Room.Info.Difficulty == DifficultyType.HD){ // HD
                    //Song info
                    //General BYTE / Float
                    buffer.WriteByte(18);//Level
                    buffer.WriteFloat(MeasureScale);//MeasureScale
                    //JudgmentDelta BYTE
                    buffer.WriteByte(3);//Kool
                    buffer.WriteByte(4);//Cool
                    buffer.WriteByte(5);//Good
                    buffer.WriteByte(6);//Miss
                    //GaugeUpDownRate Float
                    buffer.WriteFloat(0.1f);//Cool
                    buffer.WriteFloat(0.05f);//Good
                    buffer.WriteFloat(-3.0f);//Miss
                    buffer.WriteFloat(-8.0f);//Fail
                    buffer.WriteByte(0);
                }
                else if(client.Room.Info.Difficulty == DifficultyType.SHD){ // SHD
                    //Song info
                    //General BYTE / Float
                    buffer.WriteByte(20);//Level
                    buffer.WriteFloat(MeasureScale);//MeasureScale
                    //JudgmentDelta BYTE
                    buffer.WriteByte(2);//Kool
                    buffer.WriteByte(3);//Cool
                    buffer.WriteByte(4);//Good
                    buffer.WriteByte(5);//Miss
                    //GaugeUpDownRate Float
                    buffer.WriteFloat(0.1f);//Cool
                    buffer.WriteFloat(0.05f);//Good
                    buffer.WriteFloat(-4.0f);//Miss
                    buffer.WriteFloat(-10.0f);//Fail
                    buffer.WriteByte(0);
                }
                //
                buffer.WriteByte((byte)client.Room.Info.SelectedSong);//Disc Num 205
                buffer.WriteByte((byte)client.Room.Info.Difficulty);//Select difficulty  0=EZ 1=NM 2=HD 3=SHD
                buffer.WriteByte(1);
                Send(client.Room, 0x17, buffer);//23
                IBuffer announceRoomPacket = RoomPacket.CreateAnnounceRoomPacket(client.Channel);
                Send(client.Channel.GetLobbyClients(), 13, announceRoomPacket);
            }
            else{
                //Song info
                IBuffer buffer = EzServer.Buffer.Provide();
                //General BYTE / Float
                buffer.WriteByte(1);//Level
                buffer.WriteFloat(MeasureScale);//MeasureScale
                Console.WriteLine(MeasureScale);
                Console.WriteLine(MeasureScale);
                Console.WriteLine(MeasureScale);
                Console.WriteLine(MeasureScale);
                Console.WriteLine(MeasureScale);
                //JudgmentDelta BYTE
                buffer.WriteByte(8);//Kool
                buffer.WriteByte(24);//Cool
                buffer.WriteByte(60);//Good
                buffer.WriteByte(76);//Miss
                //GaugeUpDownRate Float
                buffer.WriteFloat(0.2f);//Cool
                buffer.WriteFloat(0.1f);//Good
                buffer.WriteFloat(-1.5f);//Miss
                buffer.WriteFloat(-4.0f);//Fail
                buffer.WriteByte(0);
                //
                buffer.WriteByte((byte)client.Room.Info.SelectedSong);//Disc Num 205
                buffer.WriteByte((byte)client.Room.Info.Difficulty);//Select difficulty  0=EZ 1=NM 2=HD 3=SHD
                buffer.WriteByte(1);

                Send(client.Room, 0x17, buffer);//23
                IBuffer announceRoomPacket = RoomPacket.CreateAnnounceRoomPacket(client.Channel);
                Send(client.Channel.GetLobbyClients(), 13, announceRoomPacket);                
            }
            

        }
    }
}