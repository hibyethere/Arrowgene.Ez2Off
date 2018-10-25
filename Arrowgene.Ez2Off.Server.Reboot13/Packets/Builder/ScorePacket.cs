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
using Arrowgene.Services.Buffers;

namespace Arrowgene.Ez2Off.Server.Reboot13.Packets.Builder
{
    public class ScorePacket
    {
        public static IBuffer Create(List<EzClient> clients, GameGroupType group, int exr)
        {
            short count = (short) clients.Count;

            int[] indexes = new int[count], scores = new int[count], places = new int[count];
            for(int i = 0; i < count; i++)
            {
                indexes[i] = i;
                scores[i] = clients[i].Score.TotalScore;
            }
            System.Array.Sort(scores, indexes);
            System.Array.Reverse(scores);
            System.Array.Reverse(indexes);
            int place = 0, redScore = 0, blueScore = 0;
            for (int i = 0; i < count; i++)
            {
                switch (clients[indexes[i]].Player.Team)
                {
                    case TeamType.Red:
                        redScore += clients[indexes[i]].Score.TotalScore;
                        break;
                    case TeamType.Blue:
                        blueScore += clients[indexes[i]].Score.TotalScore;
                        break;
                }
                places[0] = place;
                if (i == 0)
                {
                    places[0] = place;
                }
                else if (i > 0 && scores[i] < scores[i - 1])
                {
                    places[i] = ++place;
                }
            }
             if (group == GameGroupType.Team)
            {
                for (int i = 0; i < count; i++)
                {
                    switch (clients[indexes[i]].Player.Team)
                    {
                        case TeamType.Red:
                            if (redScore >= blueScore)
                            {
                                places[i] = 0;
                            }
                            else if (redScore < blueScore)
                            {
                                places[i] = 1;
                            }
                            break;
                        case TeamType.Blue:
                            if (blueScore >= redScore)
                            {
                                places[i] = 0;
                            }
                            else if (blueScore < redScore)
                            {
                                places[i] = 1;
                            }
                            break;
                    }
                }
            }

            IBuffer buffer = EzServer.Buffer.Provide();
            buffer.WriteInt16((short) clients.Count, Endianness.Big);
            for (short i = 0; i < count; i++)
            {
                EzClient client = clients[indexes[i]];
                Score score = client.Score;
                
                /*/
                buffer.WriteInt16(i, Endianness.Big);
                buffer.WriteByte(score.StageClear ? (byte) 0 : (byte) 1);
                buffer.WriteInt16((short) score.MaxCombo, Endianness.Big);
                buffer.WriteInt16((short) score.Kool, Endianness.Big);
                buffer.WriteInt16((short) score.Cool, Endianness.Big);
                buffer.WriteInt16((short) score.Good, Endianness.Big);
                buffer.WriteInt16((short) score.Miss, Endianness.Big);
                buffer.WriteInt16((short) score.Fail, Endianness.Big);
                */
                buffer.WriteInt16((short)score.Slot, Endianness.Big);
                buffer.WriteByte(score.StageClear ? (byte)0 : (byte)1);
                buffer.WriteInt16((short)score.MaxCombo, Endianness.Big);
                buffer.WriteInt16((short)score.Kool, Endianness.Big);
                buffer.WriteInt16((short)score.Cool, Endianness.Big);
                buffer.WriteInt16((short)score.Good, Endianness.Big);
                buffer.WriteInt16((short)score.Miss, Endianness.Big);
                buffer.WriteInt16((short)score.Fail, Endianness.Big);


                buffer.WriteByte(0);
                buffer.WriteByte((byte) score.ComboType);
                buffer.WriteInt32(score.TotalScore, Endianness.Big);
                buffer.WriteInt16(0, Endianness.Big); //+ EXP
                buffer.WriteByte((byte) score.Rank);
                buffer.WriteInt16(0, Endianness.Big); // + Coin Increase [MAX:9999]
                buffer.WriteByte(0); // 1 = Level Up [HP Points +1 / DJ Points +1] increase
                buffer.WriteInt16((short) score.TotalNotes, Endianness.Big);
                buffer.WriteByte((byte)places[i]); //尝试解决排名问题
                //buffer.WriteByte(0);
                buffer.WriteByte((byte) (places[i] > 0 ? 1 : 0));
                buffer.WriteInt16(0); //EXP +%
                buffer.WriteInt16(0); //Coin +%
                buffer.WriteByte(client.Character.Level);
                buffer.WriteInt32(client.Character.Exp);
                buffer.WriteInt32(Character.ExpForNextLevel(client.Character));
                buffer.WriteInt32(score.TotalScore > score.BestScore ? score.TotalScore : score.BestScore, Endianness.Big); // Best Score
                /*
                buffer.WriteByte(0); //Song completion EXR increase
                buffer.WriteByte((byte) client.Character.GetExr(client.Mode));
                */
                buffer.WriteByte((byte)(exr > client.Character.GetExr(client.Mode) &&
                    (score.ComboType == ComboType.AllCombo ||
                    score.ComboType == ComboType.AllCool ||
                    score.ComboType == ComboType.AllKool) ? exr : 0)); //Song completion EXR increase
                buffer.WriteByte((byte)(exr > client.Character.GetExr(client.Mode) &&
                    (score.ComboType == ComboType.AllCombo ||
                    score.ComboType == ComboType.AllCool ||
                    score.ComboType == ComboType.AllKool) ? exr : client.Character.GetExr(client.Mode)));

                if (exr > client.Character.GetExr(client.Mode) &&
                    (score.ComboType == ComboType.AllCombo ||
                    score.ComboType == ComboType.AllCool ||
                    score.ComboType == ComboType.AllKool))
                {
                    switch (client.Mode)
                    {
                        case ModeType.RubyMix:
                            client.Character.RubyExr = exr;
                            break;
                        case ModeType.StreetMix:
                            client.Character.StreetExr = exr;
                            break;
                        case ModeType.ClubMix:
                            client.Character.ClubExr = exr;
                            break;
                    }
                }
            }

            return buffer;
        }
    }
}