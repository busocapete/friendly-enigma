import React, { useState, useEffect, useRef } from "react";
import { useSelector, useDispatch } from "react-redux";
import { useLocation } from "react-router-dom";

import { Link } from "react-router-dom";

import {
  searchJobs,
  updateSearch,
  selectJobIds,
  setSearchDtoQuery,
  setSearchFilter,
  setSearchDtoSince,
  setSelectedCheckboxes,
  updateFirmTypes,
  updateIndustries,
  updateLocations,
  updateSectors,
  clearQuery,
  clearFilters,
} from "../../../features/search/jobSearchSlice";

import { Spinner } from "../../../components/common/Spinner";

import { JobSearchResult } from "./JobSearchResult";
import { ShowSinceComponent } from "../../common/search/ShowSince";
import { Facets } from "../../common/search/FacetsComponent";

export const JobSearchPage = ({}) => {
  const dispatch = useDispatch();
  const orderedJobIds = useSelector(selectJobIds);
  const location = useLocation();

  const [initialLoadComplete, setInitialLoadComplete] = useState(false);
  const previousLocation = useRef(location.pathname);

  const searchStatus = useSelector((state) => state.jobSearchResults.status);
  const searchDto = useSelector((state) => state.jobSearchResults.searchDto);
  const facets = useSelector((state) => state.jobSearchResults.facets);
  
  const firstFetchComplete = useSelector(
    (state) => state.jobSearchResults.firstFetchComplete
  );


  const error = useSelector((state) => state.feed.error);
  const totalCount = useSelector((state) => state.jobSearchResults.totalCount);

  const filter = useSelector((state) => state.jobSearchResults.filter);

  const selectedCheckboxes = useSelector(
    (state) => state.jobSearchResults.selectedCheckboxes
  );

  useEffect(() => {
    if (searchDto.search !== "" && !firstFetchComplete) {
      dispatch(searchJobs({ searchDto, filter }));
    }
  }, [searchDto, dispatch]);

  useEffect(() => {
    if (initialLoadComplete) {
      dispatch(updateSearch({ searchDto, filter }));
    }
  }, [searchDto, dispatch]);

  const handleKeyPress = (event) => {
    if (event.key === "Enter") {
      dispatch(setSearchDtoQuery(filter));
    }
  };

  useEffect(() => {
    // Check if the location changes, indicating a navigation event
    if (location.pathname !== previousLocation.current) {
      // Reset the initial load flag
      setInitialLoadComplete(false);
      // Store the new location in the ref
      previousLocation.current = location.pathname;
    } else {
      // Set the initial load to true when the component mounts
      setInitialLoadComplete(true);
    }
  }, [location]);

  const handleSearch = () => {
    dispatch(setSearchDtoQuery(filter));
  };

  const handleFilterChange = (event) => {
    const newFilter = event.target.value;
    dispatch(setSearchFilter(newFilter));
  };

  const handleSetSince = (value) => {
    dispatch(setSearchDtoSince(value));
  };

  const clearQuery = () => {
    dispatch(setSearchFilter(""));
  };

  return (
    <>
      <div className="mt-5 row justify-content-center pb-sm-2">
        <div className="mt-5 col-lg-9 col-xl-8">

          <div className="input-group mb-3">
            <input
              type="text"
              className="form-control form-control-lg"
              value={filter}
              name="filter"
              onChange={handleFilterChange}
              onKeyDown={handleKeyPress}
              required
              placeholder="Search Jobs"
              aria-label="Job Search Input Field"
            />
            <div className="input-group-append">
              {filter !== "" && (
                <button
                  className="btn btn-transparent btn-lg btn-x btn-no-border-radius"
                  type="button"
                  onClick={() => clearQuery()}
                >
                  <i className="fi-x"></i>
                </button>
              )}
              <button
                className="btn btn-primary btn-lg btn-no-left-border-radius"
                type="submit"
                onClick={() => handleSearch()}
                disabled={
                  searchStatus === "loading" || updateStatus === "loading"
                }
              >
                <i className="fi-search"></i>
              </button>
            </div>
          </div>

          {searchStatus === "succeeded" ? (
            <>
              <p className="m-2">
                <strong>
                  <em>{totalCount} search results</em>
                </strong>
              </p>

              <div className="row">
                <div className="col-md-9 pr-2">
                  <ShowSinceComponent
                    totalCount={totalCount}
                    handleSetSince={handleSetSince}
                    selectedValue={searchDto.since}
                  />

                  <Facets
                    facets={facets}
                    selectedCheckboxes={selectedCheckboxes}
                    handleSelectedCheckboxes={setSelectedCheckboxes}
                    handleUpdateIndustries={updateIndustries}
                    handleUpdateSectors={updateSectors}
                    handleUpdateLocations={updateLocations}
                    handleUpdateFirmTypes={updateFirmTypes}
                    handleClearFilters={clearFilters}
                    isMobile
                  />

                 
          ) : searchStatus === "loading" ? (
            <Spinner text="Searching jobs..." />
          ) : (
            <div>{error}</div>
          )}
        </div>
      </div>
    </>
  );
};
