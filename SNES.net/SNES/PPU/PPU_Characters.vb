﻿Partial Public Class PPU
    Private Sub RenderCharacters(Line As Integer, Pri As Integer)
        If (TM Or TS) And &H10 Then
            Dim BaseY As Integer = Line << 8
            Dim ChrBase As Integer = (ObSel And 3) << 14

            For Offset As Integer = &H1FC To 0 Step -4
                Dim X As Integer = OAM(Offset)
                Dim Y As Integer = OAM(Offset + 1)
                Dim ChrNum As Integer = OAM(Offset + 2)
                Dim Attrib As Integer = OAM(Offset + 3)

                If Y >= 224 Then Y = Y Or &HFFFFFF00
                If Line < Y Then Continue For

                ChrNum = ChrNum Or ((Attrib And 1) << 8)

                Dim PalNum As Integer = ((Attrib >> 1) And 7)
                Dim Priority As Integer = ((Attrib >> 4) And 3)
                Dim HFlip As Boolean = Attrib And &H40
                Dim VFlip As Boolean = Attrib And &H80

                If Priority = Pri Then
                    Dim OAM32Addr As Integer = &H200 + (Offset >> 4)
                    Dim OAM32Bit As Integer = (Offset And &HC) >> 1
                    Dim TSize As Boolean = OAM(OAM32Addr) And (1 << (OAM32Bit + 1))
                    Dim XHigh As Boolean = OAM(OAM32Addr) And (1 << OAM32Bit)

                    If XHigh Then X = X Or &HFFFFFF00

                    Dim TX, TY As Integer
                    Select Case ObSel >> 5
                        Case 0 : If TSize Then TX = 1 : TY = 1 Else TX = 0 : TY = 0 '8x8/16x16
                        Case 1 : If TSize Then TX = 3 : TY = 3 Else TX = 0 : TY = 0 '8x8/32x32
                        Case 2 : If TSize Then TX = 7 : TY = 7 Else TX = 0 : TY = 0 '8x8/64x64
                        Case 3 : If TSize Then TX = 3 : TY = 3 Else TX = 1 : TY = 1 '16x16/32x32
                        Case 4 : If TSize Then TX = 7 : TY = 7 Else TX = 1 : TY = 1 '16x16/64x64
                        Case 5 : If TSize Then TX = 7 : TY = 7 Else TX = 3 : TY = 3 '32x32/64x64
                        Case 6 : If TSize Then TX = 3 : TY = 7 Else TX = 1 : TY = 3 '16x32/32x64
                        Case 7 : If TSize Then TX = 3 : TY = 3 Else TX = 1 : TY = 3 '16x32/32x32
                    End Select

                    If Line >= Y + ((TY + 1) << 3) Then Continue For

                    Dim YPos As Integer = Line - Y
                    If VFlip Then YPos = YPos Xor (((TY + 1) << 3) - 1)

                    Dim TileY As Integer = YPos >> 3
                    Dim YOfs As Integer = YPos And 7

                    If HFlip Then X = X + (TX << 3)

                    For TileX As Integer = 0 To TX
                        Dim ChrAddr As Integer = ChrBase + (YOfs << 1) + ((ChrNum + (TileY << 4) + TileX) << 5)

                        For XOfs As Integer = 0 To 7
                            Dim XBit As Integer = XOfs
                            If HFlip Then XBit = XBit Xor 7
                            If X + XOfs > 255 Then Exit For
                            If X + XOfs < 0 Then Continue For

                            Dim Color As Byte = ReadChr(ChrAddr, 4, XBit)

                            If Color <> 0 Then
                                Dim BOffs As Integer = (BaseY + X + XOfs) << 2

                                Color = 128 + (PalNum << 4) + Color

                                BackBuffer(BOffs + 0) = Pal(Color).B
                                BackBuffer(BOffs + 1) = Pal(Color).G
                                BackBuffer(BOffs + 2) = Pal(Color).R
                                BackBuffer(BOffs + 3) = &HFF
                            End If
                        Next

                        If HFlip Then X = X - 8 Else X = X + 8
                    Next
                End If
            Next
        End If
    End Sub
End Class