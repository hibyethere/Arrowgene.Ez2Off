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

using System.Collections.Generic;
using Arrowgene.Ez2Off.Common.Models;
using Arrowgene.Ez2Off.Server.Client;
using Arrowgene.Ez2Off.Server.Models;
using Arrowgene.Ez2Off.Server.Packet;
using Arrowgene.Ez2Off.Server.Reboot13.Models;
using Arrowgene.Ez2Off.Server.Reboot13.Packets.Builder;
using Arrowgene.Services.Buffers;

namespace Arrowgene.Ez2Off.Server.Reboot13.Packets.World
{
    public class RoomSelectSong : Handler<WorldServer>
    {
        public RoomSelectSong(WorldServer server) : base(server)
        {
        }

        public override int Id => 9;

        public override void Handle(EzClient client, EzPacket packet)
        {
            List<EzClient> clients = client.Room.GetClients();
            foreach(var c in clients){
                if(c.Player.autoReady && !c.Player.Ready && c.Player.readyCon == 0){
                    IBuffer readybuffer = EzServer.Buffer.Provide();
                    readybuffer.WriteByte(0);
                    readybuffer.WriteByte((byte)c.Player.Slot);
                    readybuffer.WriteInt32((int) RoomOptionType.ChangeReady);
                    readybuffer.WriteInt32(1); // Ready True, False
                    readybuffer.WriteInt32(0);
                    c.Player.Ready = true;
                    IBuffer roomCharacterPacket = RoomPacket.CreateCharacterPacket(c.Room);
                    Send(c.Room, 10, roomCharacterPacket);
                    c.Player.readyCon++;
                }
            }

            RoomOptionType roomOption = (RoomOptionType) packet.Data.ReadInt32();
            _logger.Debug("Change Option: {0}", roomOption);
            IBuffer buffer = EzServer.Buffer.Provide();
            switch (roomOption)
            {
                case RoomOptionType.ChangeReady:
                {
                    bool ready = packet.Data.ReadInt32() > 0;
                    int unknown1A = packet.Data.ReadInt32();
                    _logger.Debug("ready: {0}", ready);
                    _logger.Debug("unknown1A: {0}", unknown1A); // Slot?
                    buffer.WriteByte(0);
                    buffer.WriteByte((byte)client.Player.Slot);
                    buffer.WriteInt32((int) roomOption);
                    buffer.WriteInt32(ready ? 1 : 0);
                    buffer.WriteInt32(unknown1A);
                    client.Player.Ready = ready;
                    IBuffer roomCharacterPacket = RoomPacket.CreateCharacterPacket(client.Room);
                    Send(client.Room, 10, roomCharacterPacket);
                    //client.Player.readyCon++;
                    break;
                }
                case RoomOptionType.ChangeTeam:
                {
                    TeamType team = (TeamType) packet.Data.ReadInt32();
                    int unknown0B = packet.Data.ReadInt32();
                    _logger.Debug("unknown0B: {0}", unknown0B);
                    buffer.WriteByte(0);
                    buffer.WriteByte((byte)client.Player.Slot);
                    buffer.WriteInt32((int) roomOption);
                    buffer.WriteInt32((int) team);
                    buffer.WriteInt32(unknown0B);
                    client.Player.Team = team;
                    IBuffer roomCharacterPacket = RoomPacket.CreateCharacterPacket(client.Room);
                    Send(client.Room, 10, roomCharacterPacket);
                    break;
                }
                case RoomOptionType.ChangeFade:
                    client.Room.Info.FadeEffect = (FadeEffectType) packet.Data.ReadInt32();
                    int unknown0C = packet.Data.ReadInt32();
                    _logger.Debug("unknown0C: {0}", unknown0C);
                    buffer.WriteByte(0);
                    buffer.WriteByte((byte)client.Player.Slot);
                    buffer.WriteInt32((int) roomOption);
                    buffer.WriteInt32((int) client.Room.Info.FadeEffect);
                    buffer.WriteInt32(unknown0C);
                    break;
                case RoomOptionType.ChangeNote:
                    client.Room.Info.NoteEffect = (NoteEffectType) packet.Data.ReadInt32();
                    int unknown0D = packet.Data.ReadInt32();
                    _logger.Debug("unknown0D: {0}", unknown0D);
                    buffer.WriteByte(0);
                    buffer.WriteByte((byte)client.Player.Slot);
                    buffer.WriteInt32((int) roomOption);
                    buffer.WriteInt32((int) client.Room.Info.NoteEffect);
                    buffer.WriteInt32(unknown0D);
                    break;
                case RoomOptionType.ChangeSongAndDifficulty:
                    _logger.Debug("get avatar id: {0}", client.Inventory.GetAvatarId());
                    _logger.Debug("get special id: {0}", client.Inventory.GetSpecialId());
                    _logger.Debug("get wide effect id: {0}", client.Inventory.GetWideEffectId());
                    client.Room.Info.RandomSong = false;
                    client.Room.Info.SelectedSong = packet.Data.ReadInt32();
                    client.Room.Info.Difficulty = (DifficultyType) packet.Data.ReadInt32();
                    buffer.WriteByte(0);
                    buffer.WriteByte(0);
                    buffer.WriteInt32((int) roomOption);
                    buffer.WriteInt32((int) client.Room.Info.SelectedSong);
                    buffer.WriteInt32((int) client.Room.Info.Difficulty);
                    
                    /*
                    // New Column 'ModeType' Fix 
                    for(int i = 1; i < 206; i++){
                        Song song = Database.SelectSong(i);
                        ModeType mode = client.Mode;
                        DifficultyType difficulty = client.Room.Info.Difficulty;
                        int totalNotes;
                        switch (mode)
                        {
                            case ModeType.RubyMix:
                                switch (difficulty)
                                {
                                    case DifficultyType.EZ:
                                        totalNotes = song.RubyEzNotes;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.NM:
                                        totalNotes = song.d10;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.HD:
                                        totalNotes = song.d15;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.SHD:
                                        totalNotes = song.RubyShdNotes;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                }
                                break;
                            case ModeType.StreetMix:
                                switch (difficulty)
                                {
                                    case DifficultyType.EZ:
                                        totalNotes = song.d25;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.NM:
                                        totalNotes = song.d30;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.HD:
                                        totalNotes = song.d35;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.SHD:
                                        totalNotes = song.d40;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                }
                                break;
                            case ModeType.ClubMix:
                                switch (difficulty)
                                {
                                    case DifficultyType.EZ:
                                        totalNotes = song.d45;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.NM:
                                        totalNotes = song.d50;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.HD:
                                        totalNotes = song.ClubHdNotes;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                    case DifficultyType.SHD:
                                        totalNotes = song.ClubShdNotes;
                                        Database.UpdateModeType(i, (int) difficulty, (int) mode, totalNotes);
                                        _logger.Debug("Song id: {0}", i);
                                        _logger.Debug("Song difficulty: {0}", difficulty);
                                        _logger.Debug("Song modetype: {0}", mode);
                                        _logger.Debug("Song totalnotes: {0}", totalNotes);
                                        break;
                                }
                                break;
                        }
                    }
                    */
                    break;

                case RoomOptionType.StartGame:
                    int unknown0E = packet.Data.ReadInt32();
                    int unknown1E = packet.Data.ReadInt32();
                    _logger.Debug("unknown0E: {0}", unknown0E);
                    _logger.Debug("unknown1E: {0}", unknown1E);
                    buffer.WriteByte(0);
                    buffer.WriteByte(0);
                    buffer.WriteInt32((int) roomOption);
                    buffer.WriteInt32(unknown0E);
                    buffer.WriteInt32(unknown1E);
                    break;
                case RoomOptionType.ChangeRandom:
                    client.Room.Info.RandomSong = true;
                    client.Room.Info.SelectedSong = packet.Data.ReadInt32();
                    client.Room.Info.Difficulty = (DifficultyType) packet.Data.ReadInt32();
                    buffer.WriteByte(0);
                    buffer.WriteByte(0);
                    buffer.WriteInt32((int) roomOption);
                    buffer.WriteInt32((int) client.Room.Info.SelectedSong);
                    buffer.WriteInt32((int) client.Room.Info.Difficulty);
                    break;
                case RoomOptionType.ViewVideo:
                    client.Room.Info.SelectedSong = packet.Data.ReadInt32();
                    client.Room.Info.Difficulty = (DifficultyType) packet.Data.ReadInt32();
                    buffer.WriteByte(0);
                    buffer.WriteByte(0);
                    buffer.WriteInt32((int) roomOption);
                    buffer.WriteInt32((int) client.Room.Info.SelectedSong);
                    buffer.WriteInt32((int) client.Room.Info.Difficulty);
                    // TODO deduct 1000 coins.
                    break;
            }

            Send(client.Room, 16, buffer);

            IBuffer announceRoomPacket = RoomPacket.CreateAnnounceRoomPacket(client.Channel);
            Send(client.Channel.GetLobbyClients(), 13, announceRoomPacket);

            client.Room.Log(_logger);
            
        }
    }
}