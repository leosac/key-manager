# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |

## Isolated
This application is expected to run on an isolated environment.
When dealing with a File Key Store or with Key Links, secret key values are loaded into the application memory in **clear**. Even if no effort is currently being made to reduce the footprint of such key value in memory (we are dealing with string which are immutable for instance) at the end no perfect solution will exist anyway. Just don't use File Key Store and Key Links if you have such concerns. And if you need it, isolate.

## Weak configuration
A bunch of properties are available at Key Store and Key Entry levels. You are also allowed to use deprecated crypto algorithms if supported by the targeted Key Store technology. It is your responsibility to have a strong setup.

## Reporting a Vulnerability

Please use [GitHub issues](https://github.com/leosac/key-manager/issues) directly.

As Leosac Key Manager is a standalone application expected to run on an isolated environmnent without requiring administration privileges, security issues could be discussed publicly.
Timing should matter less here but we will see over time!

