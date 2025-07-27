import React, { useState } from "react";
import { useStyles } from "./style";
import { Header, Game } from "./components";
export const WordOrderGame = () => {
    const classes = useStyles();
    return (
        <div className={classes.container}>
            <Header />
            <div>Arrange the words in the correct order</div>
            <Game />
        </div>

    );
}