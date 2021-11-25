#!/bin/bash

DEADLOCK_MINUTES=35

dotnet tool install -g dotnet-dump

while true; do
    dotnet test $@ &

    echo "Waiting for dotnet-test"
    sleep 30

    PID=$(ps axf | grep 'dotnet exec' | grep 'Tournament.Tests.dll' | grep -v grep | awk '{print $1}')
    echo "dotnet-test PID: $PID"

    for i in {1..35}; do
        echo "Waiting 1 minute for deadlock"
        sleep 60
        if ! ps -p $PID > /dev/null; then
            echo "Process exited, restarting..."
            continue 2
        fi
    done

    if ps -p $PID > /dev/null; then
        echo "Deadlocked, dumping..."
        dotnet dump collect -p $PID -o deadlock.dmp

        echo "Compressing dump..."
        tar -cjSf deadlock.tar.bz2 deadlock.dmp

        echo "Killing process..."
        kill $PID

        exit 1
    fi
done