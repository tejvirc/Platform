Principles and Practices
========================

<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

- [I. Design and Architecture Reviews](#i-design-and-architecture-reviews)
- [II. Continuous Integration](#ii-continuous-integration)
- [III. Code Reviews](#iii-code-reviews)
- [IV. Dev Testing](#iv-dev-testing)
- [V. Static Analysis and Tooling](#v-static-analysis-and-tooling)
- [VI. Dynamic Analysis – Memory and Performance Profiling](#vi-dynamic-analysis-memory-and-performance-profiling)
- [VII. Automation (Work in progress)](#vii-automation-work-in-progress)

<!-- /code_chunk_output -->

Monaco is a nice platform. We would like to keep it that way.
Accordingly, please reference these pages for best practices. It is
expected that all developers and development teams follow these
practices. This is a living document, so changes can and will occur.

## I. Design and Architecture Reviews
    1.  If there is a new feature or big change, we generally like to
        come up with a design.
    2.  We like simple designs that follow the SOLID principles.
    3.  Designs should also conform to the
        [architecture](https://confy.aristocrat.com/display/MON/High+Level+Diagrams).
    4.  Draw a picture. It helps. It doesn’t have to be fancy.
    5.  Designs should detail APIs at each layer, responsibilities, and
        dependencies.
    6.  Class and component diagrams are encouraged.
    7.  Designs should be reviewed by peers. [We have a confluence for
        designs.](https://confy.aristocrat.com/display/MON/Design).

## II. Continuous Integration
    1.  [We practice Trunk-Based
        Development.](https://trunkbaseddevelopment.com/)
    2.  [We commit to trunk daily and our commits are
        small.](https://martinfowler.com/articles/continuousIntegration.html#EveryoneCommitsToTheMainlineEveryDay)
        (\< 300 LOC)
    3.  Commits should build without warnings or failing unit tests.
    4.  Sometimes unit tests are bad and need to be put out of their
        misery. We get it.
    5.  Each dev should monitor the CI system for build failures via
        Slack and TeamCity.
    6.  If you break the build, please fix it ASAP.
    7.  Build breakers will be tarred and feathered in slack. You’ve
        been warned.

## III. Code Reviews
    1.  100% of all commits should be code reviewed by peers.
    2.  We use Upsource for platform reviews (for now). You should too.
    3.  It’s a good idea to put a dev from each team on the review.
    4.  Add your team lead as a reviewer.
    5.  Add the architects as watchers.
    6.  Reviews should be small (\<300 LOC, see principle 2.B)
    7.  Commits should conform to the [coding
        standards](https://confy.aristocrat.com/display/MON/Monaco+Coding+Standards)
        and the [code review
        guidelines](https://confy.aristocrat.com/display/MON/Code+Review+Guidelines).
    8.  All Devs should use ReSharper to identify issues before the code
        review.

## IV. Dev Testing
    1.  All code should be dev tested before a commit.
    2.  All developers should attempt to test every execution path
        before a commit.
    3.  [We like to follow the Beyonce rule of unit
        testing.](https://github.com/cpp-testing/GUnit) “If you like it
        put a test on it.”
    4.  Code without a unit test is fair game for removal. You’ve been
        warned.
    5.  Unit testing should enable rapid development, not slow it down.
    6.  Unit tests should not test private data. It defeats the point of
        an API.
    7.  There is no specific code coverage target, but more is better.
        Use your best judgment.
    8.  Devs should write integration tests. We don’t really have any
        yet, but it’d be nice…

## V. Static Analysis and Tooling
    1.  Use ReSharper to find issues in your code.
    2.  No commits should be done without running ReSharper first.
    3.  ReSharper automatically enforces the style guidelines. It makes
        life easier.
    4.  ReSharper is awesome. Hot keys are your friend. Read the manual
        and use it.
    5.  Did I mention that you should use ReSharper?
    6.  ReSharper will help you avoid getting roasted in a code review.
        I promise.

## VI. Dynamic Analysis – Memory and Performance Profiling
    1.  Use Visual Studio to monitor memory usage and performance. It
        makes life easier.
    2.  [The teams run CI builds in Robot and Super Robot mode in labs
        regularly.](https://martinfowler.com/articles/continuousIntegration.html#TestInACloneOfTheProductionEnvironment)
    3.  It is a great stress test. It finds a lot of bugs and hits the
        most common execution paths.
    4.  We have tools to record and track performance while running in
        robot mode. Use them.
    5.  Memory leaks, CPU spikes, and performance issues should be fixed
        immediately.

## VII. Automation (Work in progress)
    1.  Builds should be automated. (They are)
    2.  Dev testing should be automated. (It is)
    3.  Static analysis should be automated. (Not quite there)
    4.  Performance profiling should be automated. (95% there)
    5.  Quality Assurance Testing should be automated. (Getting there)
