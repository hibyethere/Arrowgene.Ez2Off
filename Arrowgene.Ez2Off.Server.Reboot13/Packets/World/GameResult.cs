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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arrowgene.Ez2Off.Common.Models;
using Arrowgene.Ez2Off.Server.Client;
using Arrowgene.Ez2Off.Server.Models;
using Arrowgene.Ez2Off.Server.Packet;
using Arrowgene.Ez2Off.Server.Reboot13.Models;
using Arrowgene.Ez2Off.Server.Reboot13.Packets.Builder;
using Arrowgene.Services.Buffers;

namespace Arrowgene.Ez2Off.Server.Reboot13.Packets.World
{
    public class GameResult : Handler<WorldServer>
    {
        public GameResult(WorldServer server) : base(server)
        {
        }

        public override int Id => 0x12; //18

        System.Timers.ElapsedEventHandler lastHander = null;

        public override void Handle(EzClient client, EzPacket packet)
        {
            Score score = new Score();
            score.AccountId = client.Account.Id;
            score.GameId = client.Game.Id;
            score.SongId = client.Room.Info.SelectedSong;
            score.Created = DateTime.Now;
            score.Difficulty = client.Room.Info.Difficulty;
            score.FadeEffect = client.Room.Info.FadeEffect;
            score.NoteEffect = client.Room.Info.NoteEffect;
            score.Slot = client.Player.Slot;
            score.ModeType = client.Mode;
            score.Team = client.Player.Team;

            byte unknown0 = packet.Data.ReadByte();
            score.StageClear = packet.Data.ReadByte() == 0;
            short unknown1 = packet.Data.ReadInt16(Endianness.Big);
            score.MaxCombo = packet.Data.ReadInt16(Endianness.Big);
            score.Kool = packet.Data.ReadInt16(Endianness.Big);
            score.Cool = packet.Data.ReadInt16(Endianness.Big);
            score.Good = packet.Data.ReadInt16(Endianness.Big);
            score.Miss = packet.Data.ReadInt16(Endianness.Big);
            score.Fail = packet.Data.ReadInt16(Endianness.Big);
            short unknown2 = packet.Data.ReadInt16(Endianness.Big);
            score.RawScore = packet.Data.ReadInt32(Endianness.Big);
            score.TotalNotes = packet.Data.ReadInt16(Endianness.Big);
            score.Rank = (ScoreRankType) packet.Data.ReadByte();
            byte unknown3 = packet.Data.ReadByte();
            score.ComboType = Score.GetComboType(score);
            Score bestScore = Database.SelectBestScore(score.AccountId, score.SongId, score.Difficulty, score.ModeType);
            if (bestScore != null)
                score.BestScore = bestScore.TotalScore;
            else
                score.BestScore = 0;

            _logger.Debug("StageClear: {0}", score.StageClear);
            _logger.Debug("MaxCombo: {0}", score.MaxCombo);
            _logger.Debug("Kool: {0}", score.Kool);
            _logger.Debug("Cool: {0}", score.Cool);
            _logger.Debug("Good: {0}", score.Good);
            _logger.Debug("Miss: {0}", score.Miss);
            _logger.Debug("Fail: {0}", score.Fail);
            _logger.Debug("RawScore: {0}", score.RawScore);
            _logger.Debug("TotalScore: {0}", score.TotalScore);
            _logger.Debug("Rank: {0}", score.Rank);
            _logger.Debug("Total Notes: {0}", score.TotalNotes);
            _logger.Debug("Unknown0: {0}", unknown0);
            _logger.Debug("Unknown1: {0}", unknown1);
            _logger.Debug("Unknown2: {0}", unknown2);
            _logger.Debug("Unknown3: {0}", unknown3);
            _logger.Debug("id: {0}", score.AccountId);

            client.Player.Playing = false;
            client.Score = score;
            //client.Room.Info.Playing = false; // Newly Added for checking playing state

            /* mode type add
            if(Database.UpdateModeType(score.SongId, (int) score.Difficulty, (int) score.ModeType, score.TotalNotes)){
                _logger.Debug("Song id: {0}", score.SongId);
                _logger.Debug("Song difficulty: {0}", score.Difficulty);
                _logger.Debug("Song modetype: {0}", score.ModeType);
                _logger.Debug("Song totalnotes: {0}", score.TotalNotes);
            }
            else{
                _logger.Debug("Failed");
            }
            */
            
/*
            //Play check
            IBuffer player4 = EzServer.Buffer.Provide();
            player4.WriteByte(0);

            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(100);

            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(6); //MAX COMBO

            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(7); //SCORE

            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0); //?

            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0); //?

            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0); //?

            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0);
            player4.WriteByte(0); //?
            Send(client, 0x19, player4);
            */

            List<EzClient> clients = client.Room.GetClients();
            Song song = Database.SelectSong(score.SongId);
            ModeType mode = client.Mode;
            DifficultyType difficulty = score.Difficulty;
            int exr = 0;
            int totalNotes = 0;
            switch (mode)
            {
                case ModeType.RubyMix:
                    switch (difficulty)
                    {
                        case DifficultyType.EZ:
                            exr = song.RubyEzDifficulty;
                            totalNotes = song.RubyEzNotes;
                            break;
                        case DifficultyType.NM:
                            exr = song.d8;
                            totalNotes = song.d10;
                            break;
                        case DifficultyType.HD:
                            exr = song.d13;
                            totalNotes = song.d15;
                            break;
                        case DifficultyType.SHD:
                            exr = song.RubyShdDifficulty;
                            totalNotes = song.RubyShdNotes;
                            break;
                    }
                    break;
                case ModeType.StreetMix:
                    switch (difficulty)
                    {
                        case DifficultyType.EZ:
                            exr = song.d23;
                            totalNotes = song.d25;
                            break;
                        case DifficultyType.NM:
                            exr = song.d28;
                            totalNotes = song.d30;
                            break;
                        case DifficultyType.HD:
                            exr = song.d33;
                            totalNotes = song.d35;
                            break;
                        case DifficultyType.SHD:
                            exr = song.d38;
                            totalNotes = song.d40;
                            break;
                    }
                    break;
                case ModeType.ClubMix:
                    switch (difficulty)
                    {
                        case DifficultyType.EZ:
                            exr = song.d43;
                            totalNotes = song.d45;
                            break;
                        case DifficultyType.NM:
                            exr = song.d48;
                            totalNotes = song.d50;
                            break;
                        case DifficultyType.HD:
                            exr = song.ClubHdDifficulty;
                            totalNotes = song.ClubHdNotes;
                            break;
                        case DifficultyType.SHD:
                            exr = song.d58;
                            totalNotes = song.ClubShdNotes;
                            break;
                    }
                    break;
            }

            double allKoolScore = 0;
            if ((score.BestScore == 0) || (score.TotalScore > score.BestScore)){
                // Modify: 오차 발생 가능하므로 여유있게 +10 정도. 어차피 올케쿨 보너스는 +50000
                for(int i = 1; i <= score.TotalNotes + 10; i++){ 
                    if(i >= 1443) allKoolScore += 349;
                    else allKoolScore += (170 + (17 * Math.Log(i, 2)));
                }
                _logger.Debug("allKoolScore: {0}", allKoolScore);

                if(score.RawScore <= allKoolScore){
                    if(score.SongId <= 219){
                        if(score.TotalNotes == totalNotes){
                            if (!Database.InsertScore(score))
                            {
                                _logger.Error("Could't save score for: {0}", client.Character.Name);
                            }
                        }
                    }
                    else{
                        if (!Database.InsertScore(score))
                        {
                            _logger.Error("Could't save score for: {0}", client.Character.Name);
                        }
                    } 
                }
            }

            if (client.Score.StageClear)
            {
                System.Timers.ElapsedEventHandler eventHandler = (sender, e) =>
                {
                    if (lastHander != null)
                    {
                        client.Room.GameResultWaitTimer.Elapsed -= lastHander;
                        lastHander = null;
                    }
                    OnFinished(client, clients, exr);
                };

                if (lastHander != null)
                    client.Room.GameResultWaitTimer.Elapsed -= lastHander;
                client.Room.GameResultWaitTimer.Elapsed += eventHandler;
                lastHander = eventHandler;

                client.Room.GameResultWaitTimer.Enabled = false;
                client.Room.GameResultWaitTimer.Start();
            }

            if (!client.Room.Finished())
            {
                // Last player finish will be responsible for going back to room.
                // TODO let the server check periodically incase the last person disconnectes.
                return;
            }

            if (lastHander != null)
            {
                client.Room.GameResultWaitTimer.Elapsed -= lastHander;
                lastHander = null;
            }
            client.Room.GameResultWaitTimer.Enabled = false;

            
            OnFinished(client, clients, exr);
        }

        void OnFinished(EzClient client, List<EzClient> clients, int exr){

            for(int i = clients.Count - 1; i >= 0; i--)
            {
                if (clients[i].Score == null)
                {
                    clients.RemoveAt(i);
                }
            }

            GameGroupType group = client.Room.Info.GameGroupType;
            IBuffer scorePacket = ScorePacket.Create(clients, group, exr);
            Send(client.Room, 0x1B, scorePacket); //27

            Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(t =>
            {
                
                // Display Room after 10 seconds
                foreach (var c in clients)
                {   
                    c.Player.Ready = false;
                    c.Player.readyCon = 0;
                }

                IBuffer buffer = EzServer.Buffer.Provide();
                buffer.WriteByte(0);
                Send(client.Room, 0x1C, buffer); //28

                client.Room.Info.Playing = false;
                IBuffer announceRoomPacket = RoomPacket.CreateAnnounceRoomPacket(client.Channel);
                Send(client.Channel.GetLobbyClients(), 13, announceRoomPacket);
            });
        }
           
    }
}
