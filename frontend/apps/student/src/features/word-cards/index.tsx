import { useMemo, useState } from "react";
import {
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  Typography,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { useGetWordCards, type WordCard } from "@student/api";
import { AddWordCardDialog } from "@student/components";
import { WordCardItem } from "./components";
import { useStyles } from "./style";

const mockData: WordCard[] = [
  { cardId: "1", hebrew: "שלום", english: "hello", isLearned: false },
  { cardId: "2", hebrew: "תודה", english: "thank you", isLearned: true },
  { cardId: "3", hebrew: "אור", english: "light", isLearned: false },
  { cardId: "4", hebrew: "מים", english: "water", isLearned: false },
  { cardId: "5", hebrew: "ספר", english: "book", isLearned: true },
  { cardId: "6", hebrew: "אוכל", english: "food", isLearned: false },
];

export const WordCards = () => {
  const classes = useStyles();
  const { data, isLoading, isError } = useGetWordCards();
  const [hideLearned, setHideLearned] = useState<boolean>(false);
  const [addOpen, setAddOpen] = useState<boolean>(false);

  // need to change from "mockData" to "data" when api is ready
  const filtered = useMemo(() => {
    const list = mockData ?? [];
    return hideLearned ? list.filter((c) => !c.isLearned) : list;
  }, [hideLearned]);

  return (
    <Box>
      {/* Header */}
      <Box className={classes.headerWrapper}>
        <Typography className={classes.title}>Word Cards</Typography>
        <Typography className={classes.description}>
          <strong>
            Build your own word list and practice Hebrew–English vocabulary.
          </strong>
        </Typography>
        <Typography className={classes.description}>
          Add new words, review your collection, and mark the ones you’ve
          already learned.
        </Typography>

        <Typography className={classes.helperNote}>
          <strong>Tip:</strong> You can also{" "}
          <strong>select any Hebrew word on the site</strong>. A small popup
          will let you add it directly to your cards.
        </Typography>

        <Box className={classes.headerActions}>
          <FormControlLabel
            control={
              <Checkbox
                checked={hideLearned}
                onChange={(e) => setHideLearned(e.target.checked)}
              />
            }
            label="Hide learned"
          />
          <Button
            startIcon={<AddIcon />}
            variant="contained"
            className={classes.primaryBtn}
            onClick={() => setAddOpen(true)}
          >
            Add Card
          </Button>
        </Box>
      </Box>

      {/* Body */}
      <Box className={classes.body}>
        {isLoading && !mockData ? (
          <Box className={classes.centerState}>
            <CircularProgress />
            <Typography>Loading cards…</Typography>
          </Box>
        ) : isError ? (
          <Box className={classes.centerState}>
            <Typography color="error">Failed to load word cards.</Typography>
          </Box>
        ) : filtered.length === 0 ? (
          <Box className={classes.centerState}>
            <Typography>No word cards yet.</Typography>
            <Typography variant="body2" color="text.secondary">
              Click “Add Card” or select a Hebrew word anywhere in the app to
              create one.
            </Typography>
          </Box>
        ) : (
          <Box className={classes.grid}>
            {filtered.map((card) => (
              <WordCardItem key={card.cardId} card={card} />
            ))}
          </Box>
        )}
      </Box>

      {/* Add Dialog */}
      {addOpen && (
        <AddWordCardDialog open={addOpen} onClose={() => setAddOpen(false)} />
      )}
    </Box>
  );
};
