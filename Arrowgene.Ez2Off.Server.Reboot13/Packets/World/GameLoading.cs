﻿/*
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
using Arrowgene.Ez2Off.Common;
using Arrowgene.Ez2Off.Common.Models;
using Arrowgene.Ez2Off.Server.Client;
using Arrowgene.Ez2Off.Server.Packet;
using Arrowgene.Services.Buffers;

namespace Arrowgene.Ez2Off.Server.Reboot13.Packets.World
{
    public class GameLoading : Handler<WorldServer>
    {
        public GameLoading(WorldServer server) : base(server)
        {
        }

        public override int Id => 0x10; //16

        public override void Handle(EzClient client, EzPacket packet)
        {
            //StartGameLoading
            IBuffer buffer = EzServer.Buffer.Provide();
            buffer.WriteByte(0);
            Send(client, 0x18, buffer); //24

            if(client.Game.Type == GameType.MultiPLayer){
                if(client != null && client.Player.Slot == 0){
                    Song song = Database.SelectSong(client.Room.Info.SelectedSong);
                    IBuffer response2 = EzServer.Buffer.Provide();
                    response2.WriteByte((byte) ChatType.Room);
                    response2.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                    response2.WriteByte(12);
                    response2.WriteString("BPM: " + song.Bpm.ToString(), Utils.KoreanEncoding);
                    Send(client.Room, 18, response2);
                }
            }

                    
        }
    }
}