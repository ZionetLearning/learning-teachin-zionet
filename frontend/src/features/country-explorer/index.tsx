
import React, { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useCountriesQuery } from './hooks';
import { Filters, FiltersState, Card } from './components';
import { useStyles } from './style';

const initialFilters: FiltersState = {
    search: '',
    region: 'All',
    popRange: 'ALL',
};

export const CountryExplorer: React.FC = () => {
    const { t } = useTranslation();
    const classes = useStyles();
    const [filters, setFilters] = useState<FiltersState>(initialFilters);
    const { data, isLoading, isError, error } = useCountriesQuery({
        region: filters.region,
    });

    // Client-side filters: search + population
    const list = useMemo(() => {
        let out = data ?? [];
        if (filters.region !== 'All') {
            out = out.filter(c => c.region === filters.region);
        }

        // search by name
        if (filters.search.trim()) {
            const s = filters.search.toLowerCase();
            out = out.filter(c => c.name.common.toLowerCase().includes(s));
        }

        // population
        out = out.filter(c => {
            const p = c.population ?? 0;
            switch (filters.popRange) {
                case '<10M': return p < 10_000_000;
                case '10M-100M': return p >= 10_000_000 && p < 100_000_000;
                case '>=100M': return p >= 100_000_000;
                default: return true;
            }
        });

        // sort by name for consistency
        return [...out].sort((a, b) => a.name.common.localeCompare(b.name.common));
    }, [data, filters]);

    return (
        <div className={classes.container}
        >
            <h1 className={classes.title}>{t('pages.countryExplorer.title')}</h1>
            <p className={classes.description}>
                {t('pages.countryExplorer.description')}
            </p>

            <Filters value={filters} onChange={setFilters} />
            <div className={classes.cardsWrapper}>
                {isLoading && <div>{t('pages.countryExplorer.loadingCountries')}</div>}
                {
                    isError && <div className={classes.error}>
                        {t('pages.countryExplorer.failedToLoad')} {(error as Error).message}
                    </div>
                }

                {
                    !isLoading && !isError && (
                        <div
                            className={classes.cards}
                        >
                            {list.map(c => (
                                <Card key={c.cca2 || c.name.common} country={c} />
                            ))}
                        </div>
                    )
                }
            </div>
        </div >
    );
};