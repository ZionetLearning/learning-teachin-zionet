import React, { useState, useEffect, useCallback } from "react";
import { useStyles } from "./style";
import { Speaker } from "../Speaker";
import { askAzureOpenAI } from "../../../chat/chat-yo/services";
import { useHebrewSentence } from "../../hooks";
export const Game = () => {
    const classes = useStyles();
    const [sentences, setSentences] = useState<string[]>([]);
    const [currentSentence, setCurrentSentence] = useState<string>("");
    const { loading, error, fetchSentence, setSentence } = useHebrewSentence();
    
    useEffect(() => {
        setCurrentSentence(fetchSentence());
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


