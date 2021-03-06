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

using Arrowgene.Ez2Off.Server.Client;
using Arrowgene.Ez2Off.Server.Packet;
using Arrowgene.Services.Buffers;

namespace Arrowgene.Ez2Off.Server.Solista.Packets.World
{
    public class BackButton : Handler<WorldServer>
    {
        public BackButton(WorldServer server) : base(server)
        {
        }

        public override int Id => 8;

        public override void Handle(EzClient client, EzPacket packet)
        {
            IBuffer response = EzServer.Buffer.Provide();
            response.WriteByte(0x01);
            response.WriteByte(0x00);
            response.WriteByte(0x07);
            response.WriteByte(0x00);
            response.WriteByte(0x07);
            Send(client, 5, response);

            IBuffer response1 = EzServer.Buffer.Provide();
            response1.WriteByte(00);
            response1.WriteByte(0x07);
            response1.WriteByte(00);
            response1.WriteByte(00);
            response1.WriteByte(00);
            response1.WriteByte(00);
            response1.WriteByte(0x01);
            response1.WriteByte(0x01);
            response1.WriteByte(0x01);
            response1.WriteByte(0x01);
            response1.WriteByte(00);
            response1.WriteByte(00);
            response1.WriteByte(00);
            response1.WriteByte(00);
            response1.WriteByte(0x07);
            response1.WriteByte(0x02);
            Send(client, 0X0D, response1);
        }
    }
}