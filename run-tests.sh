#!/bin/bash

DEADLOCK_TIME=2100 # 35 minutes

dotnet tool install -g dotnet-dump
dotnet test $@ &

echo "Waiting for dotnet-test"
sleep 30

PID=$(ps axf | grep 'dotnet exec' | grep 'Tournament.Tests.dll' | grep -v grep | awk '{print $1}')
echo "dotnet-test PID: $PID"

echo "Waiting $DEADLOCK_TIME seconds for deadlock"
sleep $DEADLOCK_TIME

if ps -p $PID > /dev/null; then
    echo "Deadlocked, dumping..."
    dotnet dump collect -p $PID -o deadlock.dmp

    echo "Compressing dump..."
    tar -cjSf deadlock.tar.bz2 deadlock.dmp

    echo "Killing process"
    kill $PID
fi