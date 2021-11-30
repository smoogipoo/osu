#!/bin/bash

DEADLOCK_MINUTES=35

dotnet tool install -g dotnet-dump
dotnet tool install -g dotnet-trace

get_dotnet()
{
    ps axf \
    | grep 'dotnet exec' \
    | grep 'testhost' \
    | grep -v grep \
    | tail -1
}

while true; do
    dotnet test $@ &

    for i in $(seq 1 $DEADLOCK_MINUTES); do
        echo "Waiting 1 minute for deadlock"
        sleep 60

        if [[ -z $(get_dotnet) ]]; then
            echo "Process exited, restarting..."
            continue 2
        fi
    done

    PID=$(get_dotnet | awk '{print $1}')
    echo "dotnet-test PID: $PID"

    echo "Deadlocked, dumping..."
    dotnet trace collect -p $PID --duration 00:00:00:20

    echo "Compressing dump..."
    tar -cjSf deadlock.tar.bz2 trace.nettrace

    echo "Killing process..."
    kill $PID

    exit 1
done