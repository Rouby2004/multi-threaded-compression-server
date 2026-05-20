# Multi-threaded Compression Server

A TCP Client/Server application built using C# Windows Forms that allows clients to send files to a multi-threaded server for compression using GZip compression.

---

# Project Overview

This project consists of two applications:

## 1. Compression Server
A multi-threaded TCP server that:
- Accepts multiple client connections simultaneously
- Receives files from clients
- Compresses files using GZip compression
- Sends compressed files back to clients

---

## 2. Compression Client
A Windows Forms GUI application that:
- Allows the user to select files
- Sends files to the server
- Receives compressed files
- Saves compressed files locally

---

# Features

- Multi-threaded server
- TCP socket communication
- File compression using GZip
- Windows Forms graphical interface
- Send/Receive file using size-first protocol
- Error handling
- Real-time server logs
- Support for multiple clients simultaneously

---

# Technologies Used

- C#
- .NET Framework
- Windows Forms
- TCP Sockets
- GZipStream
- Task Parallel Library (TPL)

---

# System Architecture

```text
+-------------------+          TCP Socket          +----------------------+
|   Client UI       |  ------------------------>  | Compression Server   |
|                   |                             |                      |
| Select File       |                             | Receive File         |
| Send File         |                             | Compress File        |
| Receive Zip File  | <------------------------   | Send Compressed File |
+-------------------+                             +----------------------+
