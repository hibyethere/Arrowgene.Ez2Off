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
using Arrowgene.Ez2Off.Common;
using Arrowgene.Ez2Off.Common.Models;
using Arrowgene.Ez2Off.Server.Client;
using Arrowgene.Ez2Off.Server.Packet;
using Arrowgene.Services.Buffers;
using Arrowgene.Ez2Off.Server.Models;
using Arrowgene.Ez2Off.Server.Reboot13.Packets.Builder;

namespace Arrowgene.Ez2Off.Server.Reboot13.Packets.World
{
    public class LobbyChat : Handler<WorldServer>
    {
        public LobbyChat(WorldServer server) : base(server)
        {
        }

        public override int Id => 11;

        public override void Handle(EzClient client, EzPacket packet)
        {
            ChatType chatType = (ChatType) packet.Data.ReadByte();
            string sender = packet.Data.ReadFixedString(17, Utils.KoreanEncoding);
            byte messageLength = packet.Data.ReadByte();
            string message = packet.Data.ReadString(messageLength, Utils.KoreanEncoding);
            string command;

            if (sender != client.Character.Name)
            {
                _logger.Error("Sender ({0}) does not match client name ({1})", sender, client.Character.Name);
            }

            IBuffer response = EzServer.Buffer.Provide();
            response.WriteByte((byte) chatType);
            response.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
            response.WriteByte(messageLength);
            response.WriteString(message, Utils.KoreanEncoding);

            switch (chatType)
            {
                case ChatType.Lobby:
                    try{
                        string[] r_command = message.Split(' ');
                        if(r_command[0].Equals("~rank")){
                            int songId = Int32.Parse(r_command[1]);
                            int difficulty = Int32.Parse(r_command[2]);
                            Score myBestScore = Database.SelectMyRanking(client.Account.Id, songId, difficulty, client.Mode);
                            Score totalScore = Database.SelectBestScore(client.Account.Id, songId, (DifficultyType) difficulty, client.Mode);
                            Song song = Database.SelectSong(songId);
                            string song_name = song.Name;
                            string song_info = client.Mode + " " + (DifficultyType) difficulty + " " + totalScore.TotalScore.ToString();
                            string judge = myBestScore.Kool.ToString() + "/" + myBestScore.Cool.ToString() + "/" 
                                            + myBestScore.Good.ToString() + "/" + myBestScore.Miss.ToString() + "/" + myBestScore.Fail.ToString();
                            string result = "Rank: #" + myBestScore.Ranking.ToString();

                            IBuffer r_response = EzServer.Buffer.Provide();
                            r_response.WriteByte((byte) chatType);
                            r_response.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                            r_response.WriteByte((byte) result.Length);
                            r_response.WriteString(result, Utils.KoreanEncoding);

                            IBuffer r_response2 = EzServer.Buffer.Provide();
                            r_response2.WriteByte((byte) chatType);
                            r_response2.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                            int name_len = song_name.Length*2;
                            r_response2.WriteByte((byte) name_len);
                            r_response2.WriteString(song_name, Utils.KoreanEncoding);

                            IBuffer r_response3 = EzServer.Buffer.Provide();
                            r_response3.WriteByte((byte) chatType);
                            r_response3.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                            r_response3.WriteByte((byte) song_info.Length);
                            r_response3.WriteString(song_info, Utils.KoreanEncoding);

                            IBuffer r_response4 = EzServer.Buffer.Provide();
                            r_response4.WriteByte((byte) chatType);
                            r_response4.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                            r_response4.WriteByte((byte) judge.Length);
                            r_response4.WriteString(judge, Utils.KoreanEncoding);

                            Send(client.Channel.GetLobbyClients(), 18, response);
                            Send(client.Channel.GetLobbyClients(), 18, r_response2);
                            Send(client.Channel.GetLobbyClients(), 18, r_response3);
                            Send(client.Channel.GetLobbyClients(), 18, r_response4);
                            Send(client.Channel.GetLobbyClients(), 18, r_response);
                            
                        }
                        //else if(client.Character.Name == "루뽀"){
                        else if(client.Account.State == AccountState.GameMaster){ // DB에서 Account 테이블 State 컬럼 100으로 수정
                            if(r_command[0].Equals("~delete")){
                                string name = r_command[1];
                                Console.WriteLine(name);
                                Database.DeleteScore(name.Substring(0, name.Length-1));

                                IBuffer r_response = EzServer.Buffer.Provide();
                                r_response.WriteByte((byte) chatType);
                                r_response.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                                r_response.WriteByte(10);
                                r_response.WriteString("Success", Utils.KoreanEncoding);
                                Send(client.Channel.GetLobbyClients(), 18, r_response);
                            }
                            else if(r_command[0].Equals("~d")){
                                int songId = Int32.Parse(r_command[1]);
                                int difficulty = Int32.Parse(r_command[2]);
                                int delete_score = Int32.Parse(r_command[3]);

                                Database.DeleteScore(songId, difficulty, (int) client.Mode, delete_score);

                                IBuffer r_response = EzServer.Buffer.Provide();
                                r_response.WriteByte((byte) chatType);
                                r_response.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                                r_response.WriteByte(10);
                                r_response.WriteString("Success", Utils.KoreanEncoding);
                                Send(client.Channel.GetLobbyClients(), 18, r_response);

                            }
                            else{
                                Send(client.Channel.GetLobbyClients(), 18, response);
                            }
                        }
                        else{
                            Send(client.Channel.GetLobbyClients(), 18, response);
                        }
                    }
                    catch{
                        string error_message = "Not played yet or wrong command";
                        IBuffer error_response = EzServer.Buffer.Provide();
                        error_response.WriteByte((byte) chatType);
                        error_response.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                        error_response.WriteByte((byte) error_message.Length);
                        error_response.WriteString(error_message, Utils.KoreanEncoding);
                        Send(client.Channel.GetLobbyClients(), 18, response);
                        Send(client.Channel.GetLobbyClients(), 18, error_response);
                    }
                    break;
                case ChatType.Room:
                    if(client.Player.Slot > 0){
                        try{
                            command = message.Substring(0, (int) messageLength - 1);
                            if(command.Equals("~ar")){
                                if(!client.Player.autoReady){
                                    client.Player.autoReady = true;
                                    IBuffer response2 = EzServer.Buffer.Provide();
                                    response2.WriteByte((byte) chatType);
                                    response2.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                                    response2.WriteByte(12);
                                    response2.WriteString("AutoReady ON", Utils.KoreanEncoding);
                                    Send(client.Room, 18, response);
                                    Send(client.Room, 18, response2);
                                }
                                else{
                                    client.Player.autoReady = false;
                                    IBuffer response3 = EzServer.Buffer.Provide();
                                    response3.WriteByte((byte) chatType);
                                    response3.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                                    response3.WriteByte(13);
                                    response3.WriteString("AutoReady OFF", Utils.KoreanEncoding);
                                    Send(client.Room, 18, response);
                                    Send(client.Room, 18, response3);
                                }
                            }
                            else{
                                Send(client.Room, 18, response);
                            }    
                        }
                        catch{
                            Send(client.Room, 18, response);
                        }
                    }
                    else{ // Only Master
                        try{
                            string[] k_command = message.Split(' ');
                            if(k_command[0].Equals("~kick")){

                                int userSlot = Int32.Parse(k_command[1]);

                                if(userSlot > 1){
                                    Room masterRoom = client.Room;
                                    EzClient selectedClient = masterRoom.GetClient(userSlot - 1);

                                    if(selectedClient != null){
                                        IBuffer k_response = EzServer.Buffer.Provide();
                                        k_response.WriteByte(1);
                                        k_response.WriteByte(0);
                                        k_response.WriteByte(7);
                                        k_response.WriteByte(0);
                                        k_response.WriteByte((byte) selectedClient.Session.ChannelId); //1-xCH / 0=1ch 1=2ch 2=3ch 3=4ch
                                        Send(selectedClient, 5, k_response);

                                        if (selectedClient.Room != null)
                                        {
                                            Room room = selectedClient.Room;
                                            room.Leave(selectedClient);

                                            IBuffer roomCharacterPacket = RoomPacket.CreateCharacterPacket(room);
                                            Send(room, 10, roomCharacterPacket);

                                            _logger.Debug("Character {0} left room {1}", selectedClient.Character.Name, room.Info.Name);
                                        }
                                        else
                                        {
                                            _logger.Error("Character {0} left NULL room", selectedClient.Character.Name);
                                        }
                                        
                                        IBuffer announceRoomPacket = RoomPacket.CreateAnnounceRoomPacket(selectedClient.Channel);
                                        Send(selectedClient.Channel.GetLobbyClients(), 13, announceRoomPacket);

                                        IBuffer characterList = LobbyCharacterListPacket.Create(selectedClient.Channel);
                                        Send(selectedClient.Channel.GetLobbyClients(), 2, characterList);
                                        Send(client.Room, 18, response);
                                    }
                                    else{
                                        string error_message = "NullIndex";
                                        IBuffer error_response = EzServer.Buffer.Provide();
                                        error_response.WriteByte((byte) chatType);
                                        error_response.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                                        error_response.WriteByte((byte) error_message.Length);
                                        error_response.WriteString(error_message, Utils.KoreanEncoding);
                                        Send(client.Room, 18, response);
                                        Send(client.Room, 18, error_response);
                                    }

                                }
                                else{
                                    Send(client.Room, 18, response);
                                }
                            }

                            else{
                                Send(client.Room, 18, response);
                            }
                            
                        }
                        catch{
                            string error_message = "Wrong command";
                            IBuffer error_response = EzServer.Buffer.Provide();
                            error_response.WriteByte((byte) chatType);
                            error_response.WriteFixedString(client.Character.Name, 17, Utils.KoreanEncoding);
                            error_response.WriteByte((byte) error_message.Length);
                            error_response.WriteString(error_message, Utils.KoreanEncoding);
                            Send(client.Room, 18, response);
                            Send(client.Room, 18, error_response);
                        }
                        
                    }
                    break;
                default:
                    _logger.Debug("Unknown Chat Type: {0}", chatType);
                    break;
            }
        }
    }
}