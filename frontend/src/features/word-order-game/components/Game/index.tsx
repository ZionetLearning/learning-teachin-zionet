import React, { useState, useEffect } from "react";
import { useStyles } from "./style";
import { Speaker } from "../Speaker";
export const Game = () => {
    const classes = useStyles();
    const [sentences, setSentences] = useState<string[]>([]);

    useEffect(() => {

    }, []);
    
    const handlePlay = () => {
        if (sentences.length === 0) {
            // fetch sentence from azure TTS
        }
        // play the sentence
    }

    const handleNextButton = () => {
        

    }

    return (
        <div className={classes.gameContainer}>
            <div>
                <div className={classes.speakersContainer}>
                    <Speaker />
                    <Speaker />
                </div>

                <div>
                    Here will be the sentence
                </div>

                <div className={classes.wordsBank}>
                    
                </div>

            </div>

            <div className={classes.sideButtons}>
                <button>Reset</button>
                <button>Check</button>
                <button>Next</button>
            
            </div>
        
        </div>
    );
}


